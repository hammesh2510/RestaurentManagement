using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Interfaces;
using RestaurantManagementSystem.Models;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetWithMenuItemsAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> CategoryExistsAsync(string name, int? excludeId = null)
        {
            string lowercaseName = name.Trim().ToLower();
            if (excludeId.HasValue)
            {
                return await _context.Categories.AnyAsync(c => c.Name.ToLower() == lowercaseName && c.Id != excludeId.Value);
            }
            return await _context.Categories.AnyAsync(c => c.Name.ToLower() == lowercaseName);
        }
    }
}
