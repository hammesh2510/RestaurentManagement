using RestaurantManagementSystem.Models;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetWithMenuItemsAsync(int id);
        Task<bool> CategoryExistsAsync(string name, int? excludeId = null);
    }
}
