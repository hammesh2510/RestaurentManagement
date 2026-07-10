using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Interfaces;
using RestaurantManagementSystem.ViewModels;
using System;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var detailsModel = await _categoryService.GetCategoryDetailsAsync(id);
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
            if (ModelState.IsValid)
            {
                if (!await _categoryService.IsNameUniqueAsync(model.Name))
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    return View(model);
                }

                var success = await _categoryService.CreateCategoryAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Category created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["ErrorMessage"] = "Failed to create category. Please try again.";
            }
            return View(model);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _categoryService.GetCategoryByIdAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }
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

            if (ModelState.IsValid)
            {
                if (!await _categoryService.IsNameUniqueAsync(model.Name, model.Id))
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    return View(model);
                }

                var success = await _categoryService.UpdateCategoryAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Category updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = "Failed to update category.";
            }
            return View(model);
        }

        // POST: Category/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var success = await _categoryService.ToggleActiveStatusAsync(id);
            if (success)
            {
                return Json(new { success = true, message = "Status updated successfully!" });
            }
            return Json(new { success = false, message = "Failed to update status." });
        }

        // POST: Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Category deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete category.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the category.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
