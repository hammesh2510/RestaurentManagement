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
            // Revenue: Sum of completed payments
            decimal totalRevenue = await _context.Payments
                .Where(p => p.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(p => p.AmountPaid);

            // Expenses: Sum of operational expenses + sum of supplier purchases
            decimal operationalExpenses = await _context.Expenses.SumAsync(e => e.Amount);
            decimal supplierPurchases = await _context.Purchases.SumAsync(p => p.TotalAmount);
            decimal totalExpenses = operationalExpenses + supplierPurchases;

            decimal netProfit = totalRevenue - totalExpenses;

            // Stats
            int activeOrders = await _context.Orders
                .CountAsync(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);

            int totalTables = await _context.RestaurantTables.CountAsync();
            int occupiedTables = await _context.RestaurantTables.CountAsync(t => t.Status == TableStatus.Occupied);
            double occupancyRate = totalTables > 0 ? Math.Round((double)occupiedTables / totalTables * 100, 1) : 0;

            // Chart Data - Revenue Method breakdown
            var cashSales = await _context.Payments
                .Where(p => p.PaymentStatus == PaymentStatus.Completed && p.PaymentMethod == PaymentMethod.Cash)
                .SumAsync(p => p.AmountPaid);
            var cardSales = await _context.Payments
                .Where(p => p.PaymentStatus == PaymentStatus.Completed && p.PaymentMethod == PaymentMethod.Card)
                .SumAsync(p => p.AmountPaid);
            var upiSales = await _context.Payments
                .Where(p => p.PaymentStatus == PaymentStatus.Completed && p.PaymentMethod == PaymentMethod.UPI)
                .SumAsync(p => p.AmountPaid);

            ViewBag.CashSales = cashSales;
            ViewBag.CardSales = cardSales;
            ViewBag.UpiSales = upiSales;

            // Chart Data - Expenses breakdown
            var expensesByCategory = await _context.Expenses
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount) })
                .ToDictionaryAsync(x => x.Category, x => x.Amount);

            ViewBag.ExpensesByCategory = expensesByCategory;

            // Recent Orders
            var recentOrders = await _context.Orders
                .Include(o => o.Table)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Recent Expenses
            var recentExpenses = await _context.Expenses
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
            var tables = await _context.RestaurantTables.ToListAsync();

            // Get active orders (not completed, not cancelled) and map to table
            var activeOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            ViewBag.ActiveOrders = activeOrders;

            return View(tables);
        }

        // GET: Manager/Expenses
        public async Task<IActionResult> Expenses()
        {
            var expenses = await _context.Expenses.OrderByDescending(e => e.Date).ToListAsync();
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
            var expense = await _context.Expenses.FindAsync(id);
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
