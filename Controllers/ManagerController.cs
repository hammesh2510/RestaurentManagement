using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Manager/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            // Revenue: Sum of completed payments
            decimal totalRevenue = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaymentStatus == PaymentStatus.Completed && p.Order!.RestaurantId == restaurantId)
                .SumAsync(p => p.AmountPaid);

            // Expenses: Sum of operational expenses + sum of supplier purchases
            decimal operationalExpenses = await _context.Expenses
                .Where(e => e.RestaurantId == restaurantId)
                .SumAsync(e => e.Amount);
            decimal supplierPurchases = await _context.Purchases
                .Where(p => p.RestaurantId == restaurantId)
                .SumAsync(p => p.TotalAmount);
            decimal totalExpenses = operationalExpenses + supplierPurchases;

            decimal netProfit = totalRevenue - totalExpenses;

            // Stats
            int activeOrders = await _context.Orders
                .Where(o => o.RestaurantId == restaurantId && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
                .CountAsync();

            int totalTables = await _context.RestaurantTables.Where(t => t.RestaurantId == restaurantId).CountAsync();
            int occupiedTables = await _context.RestaurantTables.Where(t => t.RestaurantId == restaurantId).CountAsync(t => t.Status == TableStatus.Occupied);
            double occupancyRate = totalTables > 0 ? Math.Round((double)occupiedTables / totalTables * 100, 1) : 0;

            // Chart Data - Revenue Method breakdown
            var cashSales = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaymentStatus == PaymentStatus.Completed && p.PaymentMethod == PaymentMethod.Cash && p.Order!.RestaurantId == restaurantId)
                .SumAsync(p => p.AmountPaid);
            var cardSales = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaymentStatus == PaymentStatus.Completed && p.PaymentMethod == PaymentMethod.Card && p.Order!.RestaurantId == restaurantId)
                .SumAsync(p => p.AmountPaid);
            var upiSales = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaymentStatus == PaymentStatus.Completed && p.PaymentMethod == PaymentMethod.UPI && p.Order!.RestaurantId == restaurantId)
                .SumAsync(p => p.AmountPaid);

            ViewBag.CashSales = cashSales;
            ViewBag.CardSales = cardSales;
            ViewBag.UpiSales = upiSales;

            // Chart Data - Expenses breakdown
            var expensesByCategory = await _context.Expenses
                .Where(e => e.RestaurantId == restaurantId)
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount) })
                .ToDictionaryAsync(x => x.Category, x => x.Amount);

            ViewBag.ExpensesByCategory = expensesByCategory;

            // Recent Orders
            var recentOrders = await _context.Orders
                .Include(o => o.Table)
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Recent Expenses
            var recentExpenses = await _context.Expenses
                .Where(e => e.RestaurantId == restaurantId)
                .OrderByDescending(e => e.Date)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.NetProfit = netProfit;
            ViewBag.ActiveOrders = activeOrders;
            ViewBag.TotalTables = totalTables;
            ViewBag.OccupiedTables = occupiedTables;
            ViewBag.OccupancyRate = occupancyRate;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.RecentExpenses = recentExpenses;

            return View();
        }

        // GET: Manager/TableActivities
        public async Task<IActionResult> TableActivities()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var tables = await _context.RestaurantTables.Where(t => t.RestaurantId == restaurantId).ToListAsync();

            // Get active orders (not completed, not cancelled) and map to table
            var activeOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.RestaurantId == restaurantId && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            ViewBag.ActiveOrders = activeOrders;

            return View(tables);
        }

        // GET: Manager/Expenses
        public async Task<IActionResult> Expenses()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var restaurantId = user?.RestaurantId ?? string.Empty;
            var expenses = await _context.Expenses
                .Where(e => e.RestaurantId == restaurantId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
            return View(expenses);
        }

        // GET: Manager/CreateExpense
        public IActionResult CreateExpense()
        {
            return View();
        }

        // POST: Manager/CreateExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(Expense expense)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            expense.RestaurantId = user?.RestaurantId ?? string.Empty;
            ModelState.Remove("RestaurantId");

            if (ModelState.IsValid)
            {
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Expense '{expense.ExpenseName}' logged successfully!";
                return RedirectToAction(nameof(Expenses));
            }
            return View(expense);
        }

        // POST: Manager/DeleteExpense/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var restaurantId = user?.RestaurantId ?? string.Empty;
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.RestaurantId == restaurantId);
            if (expense == null)
            {
                TempData["ErrorMessage"] = "Expense not found.";
                return RedirectToAction(nameof(Expenses));
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Expense deleted successfully!";
            return RedirectToAction(nameof(Expenses));
        }
    }
}
