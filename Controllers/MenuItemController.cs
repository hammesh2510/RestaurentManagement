using Microsoft.AspNetCore.Authorization;
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

        public MenuItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MenuItem
        public async Task<IActionResult> Index()
        {
            var menuItems = await _context.MenuItems.Include(m => m.Category).ToListAsync();
            return View(menuItems);
        }

        // GET: MenuItem/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: MenuItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItem menuItem)
        {
            if (ModelState.IsValid)
            {
                menuItem.CreatedAt = DateTime.UtcNow;
                menuItem.UpdatedAt = DateTime.UtcNow;

                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Menu item '{menuItem.Name}' added successfully!";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", menuItem.CategoryId);
            return View(menuItem);
        }

        // GET: MenuItem/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                TempData["ErrorMessage"] = "Menu item not found.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
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

            if (ModelState.IsValid)
            {
                try
                {
                    menuItem.UpdatedAt = DateTime.UtcNow;
                    _context.Update(menuItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Menu item '{menuItem.Name}' updated successfully!";
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

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", menuItem.CategoryId);
            return View(menuItem);
        }

        // POST: MenuItem/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                TempData["ErrorMessage"] = "Menu item not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Menu item '{menuItem.Name}' deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: MenuItem/ToggleAvailability/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
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
