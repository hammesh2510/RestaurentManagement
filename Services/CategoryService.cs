using RestaurantManagementSystem.Interfaces;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDetailsViewModel>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var resultList = new List<CategoryDetailsViewModel>();

            foreach (var category in categories)
            {
                // We fetch the full details including menu items count
                var catWithItems = await _categoryRepository.GetWithMenuItemsAsync(category.Id);
                resultList.Add(new CategoryDetailsViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt,
                    MenuItemCount = catWithItems?.MenuItems?.Count ?? 0
                });
            }

            return resultList.OrderByDescending(c => c.CreatedAt);
        }

        public async Task<CategoryViewModel?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };
        }

        public async Task<CategoryDetailsViewModel?> GetCategoryDetailsAsync(int id)
        {
            var category = await _categoryRepository.GetWithMenuItemsAsync(id);
            if (category == null) return null;

            return new CategoryDetailsViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                MenuItemCount = category.MenuItems?.Count ?? 0
            };
        }

        public async Task<bool> CreateCategoryAsync(CategoryViewModel model)
        {
            if (await _categoryRepository.CategoryExistsAsync(model.Name))
            {
                return false;
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            return await _categoryRepository.SaveChangesAsync();
        }

        public async Task<bool> UpdateCategoryAsync(CategoryViewModel model)
        {
            var category = await _categoryRepository.GetByIdAsync(model.Id);
            if (category == null) return false;

            if (await _categoryRepository.CategoryExistsAsync(model.Name, model.Id))
            {
                return false;
            }

            category.Name = model.Name.Trim();
            category.Description = model.Description?.Trim();
            category.IsActive = model.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            _categoryRepository.Update(category);
            return await _categoryRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetWithMenuItemsAsync(id);
            if (category == null) return false;

            // Restrict deletion if there are menu items associated with this category
            if (category.MenuItems != null && category.MenuItems.Any())
            {
                throw new InvalidOperationException("Cannot delete category containing active menu items.");
            }

            _categoryRepository.Delete(category);
            return await _categoryRepository.SaveChangesAsync();
        }

        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            _categoryRepository.Update(category);
            return await _categoryRepository.SaveChangesAsync();
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            return !await _categoryRepository.CategoryExistsAsync(name, excludeId);
        }
    }
}
