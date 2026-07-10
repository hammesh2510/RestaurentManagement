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
                    new RestaurantTable { TableNumber = "Table 3", Capacity = 4, Status = TableStatus.Occupied },
                    new RestaurantTable { TableNumber = "Table 4", Capacity = 6, Status = TableStatus.Available },
                    new RestaurantTable { TableNumber = "Table 5", Capacity = 6, Status = TableStatus.Reserved },
                    new RestaurantTable { TableNumber = "Table 6", Capacity = 8, Status = TableStatus.Available },
                    new RestaurantTable { TableNumber = "Table 7", Capacity = 2, Status = TableStatus.Available },
                    new RestaurantTable { TableNumber = "Table 8", Capacity = 4, Status = TableStatus.Available }
                };

                await context.RestaurantTables.AddRangeAsync(tables);
                await context.SaveChangesAsync();
            }

            // 6. Seed Expenses
            if (!context.Expenses.Any())
            {
                var expenses = new List<Expense>
                {
                    new Expense { ExpenseName = "Monthly Shop Rent", Amount = 2500.00m, Category = "Rent", Description = "Rent for the primary restaurant shop space.", Date = DateTime.UtcNow.AddDays(-5) },
                    new Expense { ExpenseName = "Employee Salaries", Amount = 4500.00m, Category = "Salaries", Description = "Salaries for kitchen staff, waiters, and manager.", Date = DateTime.UtcNow.AddDays(-2) },
                    new Expense { ExpenseName = "Fresh Veggies and Groceries", Amount = 650.00m, Category = "Ingredients", Description = "Weekly inventory purchase for spices and vegetables.", Date = DateTime.UtcNow.AddDays(-4) },
                    new Expense { ExpenseName = "Meat and Dairy Supplies", Amount = 700.00m, Category = "Ingredients", Description = "Weekly block purchase for chicken, beef, milk, cheese.", Date = DateTime.UtcNow.AddDays(-3) },
                    new Expense { ExpenseName = "Electric & Gas Bills", Amount = 480.00m, Category = "Utilities", Description = "Power consumption for air conditioning and cooking gas.", Date = DateTime.UtcNow.AddDays(-10) }
                };

                await context.Expenses.AddRangeAsync(expenses);
                await context.SaveChangesAsync();
            }

            // 7. Seed Past Completed Orders and Payments
            if (!context.Orders.Any())
            {
                var items = context.MenuItems.ToList();
                var table1 = context.RestaurantTables.First(t => t.TableNumber == "Table 1");
                var table2 = context.RestaurantTables.First(t => t.TableNumber == "Table 2");
                var creatorId = managerUser?.Id ?? waiterUser?.Id ?? "system";

                // Order 1 (Dine In, Table 1) - Completed Yesterday
                var pizza = items.First(i => i.Name == "Veg Margherita Pizza");
                var soda = items.First(i => i.Name == "Fresh Lime Soda");

                decimal sub1 = pizza.Price + (2 * soda.Price);
                decimal tax1 = Math.Round(sub1 * 0.10m, 2);
                decimal grand1 = sub1 + tax1;

                var order1 = new Order
                {
                    OrderNumber = "ORD-" + DateTime.UtcNow.AddDays(-1).ToString("yyyyMMdd") + "-0001",
                    TableId = table1.Id,
                    OrderType = OrderType.DineIn,
                    Status = OrderStatus.Completed,
                    SubTotal = sub1,
                    TaxAmount = tax1,
                    GrandTotal = grand1,
                    CreatedByUserId = creatorId,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    OrderNotes = "Seeded completed order 1"
                };
                await context.Orders.AddAsync(order1);
                await context.SaveChangesAsync();

                await context.OrderItems.AddRangeAsync(
                    new OrderItem { OrderId = order1.Id, MenuItemId = pizza.Id, Quantity = 1, Price = pizza.Price },
                    new OrderItem { OrderId = order1.Id, MenuItemId = soda.Id, Quantity = 2, Price = soda.Price }
                );

                await context.Payments.AddAsync(new Payment
                {
                    OrderId = order1.Id,
                    PaymentMethod = PaymentMethod.Cash,
                    AmountPaid = grand1,
                    PaymentStatus = PaymentStatus.Completed,
                    PaymentDate = DateTime.UtcNow.AddDays(-1),
                    TransactionId = "TXN-CASH-998877"
                });

                // Order 2 (Dine In, Table 2) - Completed Today
                var wings = items.First(i => i.Name == "Crispy Chicken Wings");
                var pasta = items.First(i => i.Name == "Creamy Pasta Alfredo");
                var brownie = items.First(i => i.Name == "Sizzling Chocolate Brownie");

                decimal sub2 = (2 * wings.Price) + pasta.Price + brownie.Price;
                decimal tax2 = Math.Round(sub2 * 0.10m, 2);
                decimal grand2 = sub2 + tax2;

                var order2 = new Order
                {
                    OrderNumber = "ORD-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-0002",
                    TableId = table2.Id,
                    OrderType = OrderType.DineIn,
                    Status = OrderStatus.Completed,
                    SubTotal = sub2,
                    TaxAmount = tax2,
                    GrandTotal = grand2,
                    CreatedByUserId = creatorId,
                    CreatedAt = DateTime.UtcNow.AddHours(-4),
                    OrderNotes = "Seeded completed order 2"
                };
                await context.Orders.AddAsync(order2);
                await context.SaveChangesAsync();

                await context.OrderItems.AddRangeAsync(
                    new OrderItem { OrderId = order2.Id, MenuItemId = wings.Id, Quantity = 2, Price = wings.Price },
                    new OrderItem { OrderId = order2.Id, MenuItemId = pasta.Id, Quantity = 1, Price = pasta.Price },
                    new OrderItem { OrderId = order2.Id, MenuItemId = brownie.Id, Quantity = 1, Price = brownie.Price }
                );

                await context.Payments.AddAsync(new Payment
                {
                    OrderId = order2.Id,
                    PaymentMethod = PaymentMethod.Card,
                    AmountPaid = grand2,
                    PaymentStatus = PaymentStatus.Completed,
                    PaymentDate = DateTime.UtcNow.AddHours(-4),
                    TransactionId = "TXN-CARD-123456"
                });

                // Order 3 (Takeaway) - Completed Today
                var sandwich = items.First(i => i.Name == "Chicken Club Sandwich");
                var fries = items.First(i => i.Name == "French Fries");
                var latte = items.First(i => i.Name == "Iced Café Latte");

                decimal sub3 = sandwich.Price + fries.Price + latte.Price;
                decimal tax3 = Math.Round(sub3 * 0.10m, 2);
                decimal grand3 = sub3 + tax3;

                var order3 = new Order
                {
                    OrderNumber = "ORD-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-0003",
                    TableId = null, // Takeaway
                    OrderType = OrderType.TakeAway,
                    Status = OrderStatus.Completed,
                    SubTotal = sub3,
                    TaxAmount = tax3,
                    GrandTotal = grand3,
                    CreatedByUserId = creatorId,
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    OrderNotes = "Seeded completed order 3 (Takeaway)"
                };
                await context.Orders.AddAsync(order3);
                await context.SaveChangesAsync();

                await context.OrderItems.AddRangeAsync(
                    new OrderItem { OrderId = order3.Id, MenuItemId = sandwich.Id, Quantity = 1, Price = sandwich.Price },
                    new OrderItem { OrderId = order3.Id, MenuItemId = fries.Id, Quantity = 1, Price = fries.Price },
                    new OrderItem { OrderId = order3.Id, MenuItemId = latte.Id, Quantity = 1, Price = latte.Price }
                );

                await context.Payments.AddAsync(new Payment
                {
                    OrderId = order3.Id,
                    PaymentMethod = PaymentMethod.UPI,
                    AmountPaid = grand3,
                    PaymentStatus = PaymentStatus.Completed,
                    PaymentDate = DateTime.UtcNow.AddHours(-1),
                    TransactionId = "TXN-UPI-776655"
                });

                // 8. Seed Active Order for Table 3 (Occupied)
                var table3 = context.RestaurantTables.First(t => t.TableNumber == "Table 3");
                var garlicBread = items.First(i => i.Name == "Garlic Bread with Cheese");
                var water = items.First(i => i.Name == "Mineral Water");

                decimal sub4 = pizza.Price + garlicBread.Price + (2 * water.Price);
                decimal tax4 = Math.Round(sub4 * 0.10m, 2);
                decimal grand4 = sub4 + tax4;

                var order4 = new Order
                {
                    OrderNumber = "ORD-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-0004",
                    TableId = table3.Id,
                    OrderType = OrderType.DineIn,
                    Status = OrderStatus.Preparing,
                    SubTotal = sub4,
                    TaxAmount = tax4,
                    GrandTotal = grand4,
                    CreatedByUserId = waiterUser?.Id ?? creatorId,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                    OrderNotes = "Extra spicy and well-done crust"
                };
                await context.Orders.AddAsync(order4);
                await context.SaveChangesAsync();

                await context.OrderItems.AddRangeAsync(
                    new OrderItem { OrderId = order4.Id, MenuItemId = pizza.Id, Quantity = 1, Price = pizza.Price },
                    new OrderItem { OrderId = order4.Id, MenuItemId = garlicBread.Id, Quantity = 1, Price = garlicBread.Price },
                    new OrderItem { OrderId = order4.Id, MenuItemId = water.Id, Quantity = 2, Price = water.Price }
                );
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
