using RestaurantManagementSystem.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDetailsViewModel>> GetAllCategoriesAsync();
        Task<CategoryViewModel?> GetCategoryByIdAsync(int id);
        Task<CategoryDetailsViewModel?> GetCategoryDetailsAsync(int id);
        Task<bool> CreateCategoryAsync(CategoryViewModel model);
        Task<bool> UpdateCategoryAsync(CategoryViewModel model);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> ToggleActiveStatusAsync(int id);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    }
}
