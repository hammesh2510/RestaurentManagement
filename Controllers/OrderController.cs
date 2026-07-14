using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Hubs;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // GET: Order/WaiterPOS
        [Authorize(Roles = "Manager,Waiter")]
        public async Task<IActionResult> WaiterPOS(int? tableId)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var categories = await _context.Categories.Where(c => c.IsActive && c.RestaurantId == restaurantId).ToListAsync();
            var menuItems = await _context.MenuItems.Where(m => m.IsAvailable && m.RestaurantId == restaurantId).Include(m => m.Category).ToListAsync();
            var tables = await _context.RestaurantTables.Where(t => t.RestaurantId == restaurantId).ToListAsync();

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
                table = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == model.TableId.Value && t.RestaurantId == restaurantId);
                if (table == null)
                {
                    return Json(new { success = false, message = "Selected table does not exist." });
                }
            }

            // Generate Order Number: ORD-yyyyMMdd-XXXX (count of today + 1)
            var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var orderCountToday = await _context.Orders.CountAsync(o => o.OrderNumber.Contains(todayStr) && o.RestaurantId == restaurantId);
            var orderNumber = $"ORD-{todayStr}-{(orderCountToday + 1):D4}";

            // Calculate SubTotal
            decimal subTotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in model.Items)
            {
                var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == item.MenuItemId && m.RestaurantId == restaurantId);
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

            await _hubContext.Clients.All.SendAsync("OrderUpdated");

            TempData["SuccessMessage"] = $"Order {order.OrderNumber} sent to kitchen!";

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

            // Validate backend authorization based on client status change request and user role
            if (status == OrderStatus.Preparing || status == OrderStatus.Ready)
            {
                if (!User.IsInRole("Manager") && !User.IsInRole("Chef"))
                {
                    return Json(new { success = false, message = "Only managers and chefs can start cooking or mark orders as ready." });
                }
            }
            else if (status == OrderStatus.Served)
            {
                if (!User.IsInRole("Manager") && !User.IsInRole("Waiter"))
                {
                    return Json(new { success = false, message = "Only managers and waiters can mark orders as served." });
                }
            }
            else if (status == OrderStatus.Cancelled)
            {
                if (!User.IsInRole("Manager") && !User.IsInRole("Waiter"))
                {
                    return Json(new { success = false, message = "Only managers and waiters can cancel orders." });
                }
            }

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
            await _hubContext.Clients.All.SendAsync("OrderUpdated");
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

            await _hubContext.Clients.All.SendAsync("OrderUpdated");

            TempData["SuccessMessage"] = $"Order {order.OrderNumber} checkout completed, table marked as cleaning!";
            return RedirectToAction("TableActivities", "Manager");
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Waiter")]
        public async Task<IActionResult> GetTables()
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var tables = await _context.RestaurantTables
                .Where(t => t.RestaurantId == restaurantId)
                .OrderBy(t => t.TableNumber)
                .Select(t => new
                {
                    id = t.Id,
                    tableNumber = t.TableNumber,
                    status = (int)t.Status,
                    statusText = t.Status.ToString()
                })
                .ToListAsync();

            return Json(tables);
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
