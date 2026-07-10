using Microsoft.AspNetCore.Identity;
using RestaurantManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // 1. Seed Roles
            string[] roles = { "Manager", "Cashier", "Waiter", "Chef" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Users
            ApplicationUser managerUser = await SeedUserAsync(userManager, "manager@restaurant.com", "Restaurant Manager", "Manager", "Manager@123");
            ApplicationUser waiterUser = await SeedUserAsync(userManager, "waiter@restaurant.com", "John Waiter", "Waiter", "Waiter@123");
            ApplicationUser chefUser = await SeedUserAsync(userManager, "chef@restaurant.com", "Chef Gordon", "Chef", "Chef@123");

            // Make sure the changes are committed before adding other data
            await context.SaveChangesAsync();

            // 3. Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Appetizers", Description = "Start off your meal with delicious starters", IsActive = true },
                    new Category { Name = "Main Course", Description = "Hearty and filling entrees", IsActive = true },
                    new Category { Name = "Desserts", Description = "Delectable sweet treats", IsActive = true },
                    new Category { Name = "Beverages", Description = "Refreshing hot & cold drinks", IsActive = true }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 4. Seed Menu Items
            if (!context.MenuItems.Any())
            {
                var appetizers = context.Categories.First(c => c.Name == "Appetizers");
                var mainCourse = context.Categories.First(c => c.Name == "Main Course");
                var desserts = context.Categories.First(c => c.Name == "Desserts");
                var beverages = context.Categories.First(c => c.Name == "Beverages");

                var items = new List<MenuItem>
                {
                    new MenuItem { Name = "Garlic Bread with Cheese", Price = 6.99m, IsVeg = true, IsAvailable = true, IsRecommended = true, CategoryId = appetizers.Id, Description = "Baked bread topped with garlic butter and melted mozzarella." },
                    new MenuItem { Name = "Crispy Chicken Wings", Price = 9.99m, IsVeg = false, IsAvailable = true, IsRecommended = false, CategoryId = appetizers.Id, Description = "Fried wings tossed in spicy buffalo sauce." },
                    new MenuItem { Name = "French Fries", Price = 4.99m, IsVeg = true, IsAvailable = true, IsRecommended = false, CategoryId = appetizers.Id, Description = "Crispy golden salted potato fries." },

                    new MenuItem { Name = "Veg Margherita Pizza", Price = 12.99m, IsVeg = true, IsAvailable = true, IsRecommended = true, CategoryId = mainCourse.Id, Description = "Classic pizza with tomato sauce, fresh basil, and fresh mozzarella." },
                    new MenuItem { Name = "Chicken Club Sandwich", Price = 10.49m, IsVeg = false, IsAvailable = true, IsRecommended = false, CategoryId = mainCourse.Id, Description = "Triple-decker sandwich with chicken, lettuce, tomato, and mayo." },
                    new MenuItem { Name = "Creamy Pasta Alfredo", Price = 11.99m, IsVeg = true, IsAvailable = true, IsRecommended = true, CategoryId = mainCourse.Id, Description = "Fettuccine pasta in rich buttery parmesan cream sauce." },

                    new MenuItem { Name = "Sizzling Chocolate Brownie", Price = 7.99m, IsVeg = true, IsAvailable = true, IsRecommended = true, CategoryId = desserts.Id, Description = "Hot fudge brownie served with a scoop of vanilla ice cream." },
                    new MenuItem { Name = "New York Cheesecake", Price = 8.49m, IsVeg = true, IsAvailable = true, IsRecommended = false, CategoryId = desserts.Id, Description = "Rich, creamy baked cheesecake slice." },

                    new MenuItem { Name = "Fresh Lime Soda", Price = 3.99m, IsVeg = true, IsAvailable = true, IsRecommended = false, CategoryId = beverages.Id, Description = "Chilled soda flavored with fresh lime juice and sugar syrup." },
                    new MenuItem { Name = "Iced Café Latte", Price = 4.99m, IsVeg = true, IsAvailable = true, IsRecommended = false, CategoryId = beverages.Id, Description = "Chilled espresso coffee blended with fresh milk and ice." },
                    new MenuItem { Name = "Mineral Water", Price = 1.99m, IsVeg = true, IsAvailable = true, IsRecommended = false, CategoryId = beverages.Id, Description = "Chilled bottled spring water." }
                };

                await context.MenuItems.AddRangeAsync(items);
                await context.SaveChangesAsync();
            }

            // 5. Seed Restaurant Tables
            if (!context.RestaurantTables.Any())
            {
                var tables = new List<RestaurantTable>
                {
                    new RestaurantTable { TableNumber = "Table 1", Capacity = 2, Status = TableStatus.Available },
                    new RestaurantTable { TableNumber = "Table 2", Capacity = 4, Status = TableStatus.Available },
                    new RestaurantTable { TableNumber = "Table 3", Capacity = 4, Status = TableStatus.Available },
                    new RestaurantTable { TableNumber = "Table 4", Capacity = 6, Status = TableStatus.Available },
                    new RestaurantTable { TableNumber = "Table 5", Capacity = 6, Status = TableStatus.Available },
                    new RestaurantTable { TableNumber = "Table 6", Capacity = 8, Status = TableStatus.Available },
                    new RestaurantTable { TableNumber = "Table 7", Capacity = 2, Status = TableStatus.Available },
                    new RestaurantTable { TableNumber = "Table 8", Capacity = 4, Status = TableStatus.Available }
                };

                await context.RestaurantTables.AddRangeAsync(tables);
                await context.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser> SeedUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string fullName,
            string role,
            string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            return user;
        }
    }
}
