using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MenuItemController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MenuItem
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var menuItems = await _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.RestaurantId == restaurantId)
                .ToListAsync();
            return View(menuItems);
        }

        // GET: MenuItem/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var categories = await _context.Categories
                .Where(c => c.IsActive && c.RestaurantId == restaurantId)
                .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: MenuItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItem menuItem)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;
            menuItem.RestaurantId = restaurantId;
            ModelState.Remove("RestaurantId");

            if (ModelState.IsValid)
            {
                menuItem.CreatedAt = DateTime.UtcNow;
                menuItem.UpdatedAt = DateTime.UtcNow;

                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Menu item '{menuItem.Name}' added successfully!";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories
                .Where(c => c.IsActive && c.RestaurantId == restaurantId)
                .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", menuItem.CategoryId);
            return View(menuItem);
        }

        // GET: MenuItem/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.RestaurantId == restaurantId);
            if (menuItem == null)
            {
                TempData["ErrorMessage"] = "Menu item not found.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories
                .Where(c => c.IsActive && c.RestaurantId == restaurantId)
                .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", menuItem.CategoryId);
            return View(menuItem);
        }

        // POST: MenuItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItem menuItem)
        {
            if (id != menuItem.Id)
            {
                TempData["ErrorMessage"] = "Mismatched request ID.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var existingItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.RestaurantId == restaurantId);
            if (existingItem == null)
            {
                TempData["ErrorMessage"] = "Menu item not found.";
                return RedirectToAction(nameof(Index));
            }

            menuItem.RestaurantId = restaurantId;
            ModelState.Remove("RestaurantId");

            if (ModelState.IsValid)
            {
                try
                {
                    existingItem.Name = menuItem.Name;
                    existingItem.Price = menuItem.Price;
                    existingItem.CategoryId = menuItem.CategoryId;
                    existingItem.Description = menuItem.Description;
                    existingItem.IsVeg = menuItem.IsVeg;
                    existingItem.IsAvailable = menuItem.IsAvailable;
                    existingItem.IsRecommended = menuItem.IsRecommended;
                    existingItem.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Menu item '{existingItem.Name}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.MenuItems.AnyAsync(m => m.Id == menuItem.Id))
                    {
                        TempData["ErrorMessage"] = "Menu item no longer exists.";
                        return RedirectToAction(nameof(Index));
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories
                .Where(c => c.IsActive && c.RestaurantId == restaurantId)
                .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", menuItem.CategoryId);
            return View(menuItem);
        }

        // POST: MenuItem/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.RestaurantId == restaurantId);
            if (menuItem == null)
            {
                TempData["ErrorMessage"] = "Menu item not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Cascade delete associated order items in code to avoid Restrict violation
                var referencingOrderItems = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Where(oi => oi.MenuItemId == id && oi.Order!.RestaurantId == restaurantId)
                    .ToListAsync();

                if (referencingOrderItems.Any())
                {
                    var orderIds = referencingOrderItems.Select(oi => oi.OrderId).Distinct().ToList();
                    _context.OrderItems.RemoveRange(referencingOrderItems);

                    // Clean up blank orders/payments if the order has no other items left
                    foreach (var orderId in orderIds)
                    {
                        var otherItemsCount = await _context.OrderItems.CountAsync(oi => oi.OrderId == orderId && oi.MenuItemId != id);
                        if (otherItemsCount == 0)
                        {
                            var payments = await _context.Payments.Where(p => p.OrderId == orderId).ToListAsync();
                            if (payments.Any())
                            {
                                _context.Payments.RemoveRange(payments);
                            }
                            var order = await _context.Orders.FindAsync(orderId);
                            if (order != null)
                            {
                                _context.Orders.Remove(order);
                            }
                        }
                    }
                }

                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Menu item '{menuItem.Name}' deleted successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the menu item.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: MenuItem/ToggleAvailability/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.RestaurantId == restaurantId);
            if (menuItem == null)
            {
                return Json(new { success = false, message = "Item not found." });
            }

            menuItem.IsAvailable = !menuItem.IsAvailable;
            menuItem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Item updated. Availability is now: {menuItem.IsAvailable}" });
        }
    }
}
