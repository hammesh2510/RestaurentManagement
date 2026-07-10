using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var categories = await _context.Categories
                .Where(c => c.RestaurantId == restaurantId)
                .Select(c => new CategoryDetailsViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    MenuItemCount = c.MenuItems.Count
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var detailsModel = await _context.Categories
                .Include(c => c.MenuItems)
                .Where(c => c.Id == id && c.RestaurantId == restaurantId)
                .Select(c => new CategoryDetailsViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    MenuItemCount = c.MenuItems.Count
                })
                .FirstOrDefaultAsync();

            if (detailsModel == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(detailsModel);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            if (ModelState.IsValid)
            {
                var lowercaseName = model.Name.Trim().ToLower();
                var exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == lowercaseName && c.RestaurantId == restaurantId);
                if (exists)
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    return View(model);
                }

                var category = new Category
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim(),
                    IsActive = model.IsActive,
                    RestaurantId = restaurantId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };

            return View(model);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Request mismatch.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                var lowercaseName = model.Name.Trim().ToLower();
                var exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == lowercaseName && c.Id != id && c.RestaurantId == restaurantId);
                if (exists)
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    return View(model);
                }

                category.Name = model.Name.Trim();
                category.Description = model.Description?.Trim();
                category.IsActive = model.IsActive;
                category.UpdatedAt = DateTime.UtcNow;

                _context.Update(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: Category/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId);
            if (category != null)
            {
                category.IsActive = !category.IsActive;
                category.UpdatedAt = DateTime.UtcNow;

                _context.Update(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Status updated successfully!" });
            }
            return Json(new { success = false, message = "Category not found." });
        }

        // POST: Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var category = await _context.Categories.Include(c => c.MenuItems).FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            if (category.MenuItems != null && category.MenuItems.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete category containing active menu items.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
