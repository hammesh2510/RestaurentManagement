using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Order/WaiterPOS
        public async Task<IActionResult> WaiterPOS(int? tableId)
        {
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            var menuItems = await _context.MenuItems.Where(m => m.IsAvailable).Include(m => m.Category).ToListAsync();
            var tables = await _context.RestaurantTables.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.MenuItems = menuItems;
            ViewBag.Tables = tables;
            ViewBag.SelectedTableId = tableId;

            return View();
        }

        // POST: Order/PlaceOrder
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderSubmissionModel model)
        {
            if (model == null || model.Items == null || !model.Items.Any())
            {
                return Json(new { success = false, message = "Cart is empty." });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User session expired." });
            }

            var userObj = await _userManager.FindByIdAsync(userId);
            var restaurantId = userObj?.RestaurantId ?? string.Empty;

            // Verify Table
            RestaurantTable? table = null;
            if (model.OrderType == OrderType.DineIn)
            {
                if (!model.TableId.HasValue)
                {
                    return Json(new { success = false, message = "Table is required for Dine-in orders." });
                }
                table = await _context.RestaurantTables.FindAsync(model.TableId.Value);
                if (table == null)
                {
                    return Json(new { success = false, message = "Selected table does not exist." });
                }
            }

            // Generate Order Number: ORD-yyyyMMdd-XXXX (count of today + 1)
            var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var orderCountToday = await _context.Orders.CountAsync(o => o.OrderNumber.Contains(todayStr));
            var orderNumber = $"ORD-{todayStr}-{(orderCountToday + 1):D4}";

            // Calculate SubTotal
            decimal subTotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in model.Items)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                if (menuItem == null || !menuItem.IsAvailable)
                {
                    return Json(new { success = false, message = $"Item with ID {item.MenuItemId} is not available." });
                }

                var price = menuItem.Price;
                subTotal += price * item.Quantity;

                orderItems.Add(new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = item.Quantity,
                    Price = price,
                    Notes = item.Notes
                });
            }

            // Tax (default 10%)
            decimal taxRate = 0.10m;
            decimal taxAmount = Math.Round(subTotal * taxRate, 2);
            decimal grandTotal = subTotal + taxAmount;

            var order = new Order
            {
                OrderNumber = orderNumber,
                TableId = model.OrderType == OrderType.DineIn ? model.TableId : null,
                OrderType = model.OrderType,
                Status = OrderStatus.Pending,
                SubTotal = subTotal,
                DiscountAmount = 0.00m,
                TaxAmount = taxAmount,
                GrandTotal = grandTotal,
                OrderNotes = model.OrderNotes,
                CreatedByUserId = userId,
                RestaurantId = restaurantId,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            // Update physical table status
            if (table != null)
            {
                table.Status = TableStatus.Occupied;
                _context.Update(table);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, orderId = order.Id, orderNumber = order.OrderNumber });
        }

        // GET: Order/OrderBoard
        public async Task<IActionResult> OrderBoard()
        {
            var userObj = await _userManager.GetUserAsync(User);
            var restaurantId = userObj?.RestaurantId ?? string.Empty;

            var activeOrders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.RestaurantId == restaurantId && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            return View(activeOrders);
        }

        // POST: Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var userObj = await _userManager.GetUserAsync(User);
            var restaurantId = userObj?.RestaurantId ?? string.Empty;

            var order = await _context.Orders.Include(o => o.Table).FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurantId);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            order.Status = status;
            
            // If cancelled, free up the table
            if (status == OrderStatus.Cancelled && order.Table != null)
            {
                order.Table.Status = TableStatus.Available;
                _context.Update(order.Table);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Order status updated to {status}" });
        }

        // GET: Order/PrintBill/5
        public async Task<IActionResult> PrintBill(int id)
        {
            var userObj = await _userManager.GetUserAsync(User);
            var restaurantId = userObj?.RestaurantId ?? string.Empty;

            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.CreatedByUser)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurantId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(OrderBoard));
            }

            return View(order);
        }

        // GET: Order/Checkout/5
        [Authorize(Roles = "Manager,Cashier")]
        public async Task<IActionResult> Checkout(int id)
        {
            var userObj = await _userManager.GetUserAsync(User);
            var restaurantId = userObj?.RestaurantId ?? string.Empty;

            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurantId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("TableActivities", "Manager");
            }

            if (order.Status == OrderStatus.Completed)
            {
                TempData["ErrorMessage"] = "Order has already been checked out.";
                return RedirectToAction("TableActivities", "Manager");
            }

            return View(order);
        }

        // POST: Order/Checkout/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Cashier")]
        public async Task<IActionResult> Checkout(int id, PaymentMethod paymentMethod, decimal discountAmount, string? transactionId)
        {
            var userObj = await _userManager.GetUserAsync(User);
            var restaurantId = userObj?.RestaurantId ?? string.Empty;

            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurantId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("TableActivities", "Manager");
            }

            if (order.Status == OrderStatus.Completed)
            {
                TempData["ErrorMessage"] = "Order already completed.";
                return RedirectToAction("TableActivities", "Manager");
            }

            // Adjust totals for discount
            order.DiscountAmount = Math.Clamp(discountAmount, 0, order.SubTotal);
            order.GrandTotal = Math.Max(0, (order.SubTotal - order.DiscountAmount) + order.TaxAmount);
            order.Status = OrderStatus.Completed;

            // Free physical table
            if (order.Table != null)
            {
                order.Table.Status = TableStatus.Cleaning; // Set to Cleaning first
                _context.Update(order.Table);
            }

            // Create Payment record
            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMethod = paymentMethod,
                AmountPaid = order.GrandTotal,
                TransactionId = transactionId,
                PaymentStatus = PaymentStatus.Completed,
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order {order.OrderNumber} checkout completed, table marked as cleaning!";
            return RedirectToAction("TableActivities", "Manager");
        }
    }

    public class OrderSubmissionModel
    {
        public int? TableId { get; set; }
        public OrderType OrderType { get; set; }
        public string? OrderNotes { get; set; }
        public List<OrderItemSubmissionModel> Items { get; set; } = new List<OrderItemSubmissionModel>();
    }

    public class OrderItemSubmissionModel
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
}
