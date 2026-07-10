using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class TableController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TableController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Table
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var tables = await _context.RestaurantTables
                .Where(t => t.RestaurantId == restaurantId)
                .ToListAsync();
            return View(tables);
        }

        // GET: Table/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Table/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RestaurantTable table)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;
            table.RestaurantId = restaurantId;
            ModelState.Remove("RestaurantId");

            if (ModelState.IsValid)
            {
                // Check if table number is unique
                var exists = await _context.RestaurantTables.AnyAsync(t => t.TableNumber == table.TableNumber && t.RestaurantId == restaurantId);
                if (exists)
                {
                    ModelState.AddModelError("TableNumber", "A table with this number already exists.");
                    return View(table);
                }

                _context.RestaurantTables.Add(table);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Table {table.TableNumber} created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(table);
        }

        // GET: Table/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var table = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == restaurantId);
            if (table == null)
            {
                TempData["ErrorMessage"] = "Table not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(table);
        }

        // POST: Table/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RestaurantTable table)
        {
            if (id != table.Id)
            {
                TempData["ErrorMessage"] = "Mismatched request ID.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var existingTable = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == restaurantId);
            if (existingTable == null)
            {
                TempData["ErrorMessage"] = "Table not found.";
                return RedirectToAction(nameof(Index));
            }

            table.RestaurantId = restaurantId;
            ModelState.Remove("RestaurantId");

            if (ModelState.IsValid)
            {
                var exists = await _context.RestaurantTables.AnyAsync(t => t.TableNumber == table.TableNumber && t.Id != table.Id && t.RestaurantId == restaurantId);
                if (exists)
                {
                    ModelState.AddModelError("TableNumber", "A table with this number already exists.");
                    return View(table);
                }

                try
                {
                    existingTable.TableNumber = table.TableNumber;
                    existingTable.Capacity = table.Capacity;
                    existingTable.Status = table.Status;

                    _context.Update(existingTable);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Table {existingTable.TableNumber} updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.RestaurantTables.AnyAsync(t => t.Id == table.Id))
                    {
                        TempData["ErrorMessage"] = "Table no longer exists.";
                        return RedirectToAction(nameof(Index));
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(table);
        }

        // POST: Table/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var table = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == restaurantId);
            if (table == null)
            {
                TempData["ErrorMessage"] = "Table not found.";
                return RedirectToAction(nameof(Index));
            }

            // Don't delete if table is Occupied
            if (table.Status == TableStatus.Occupied)
            {
                TempData["ErrorMessage"] = "Cannot delete an occupied table.";
                return RedirectToAction(nameof(Index));
            }

            _context.RestaurantTables.Remove(table);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Table {table.TableNumber} deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Table/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, TableStatus status)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurantId = user?.RestaurantId ?? string.Empty;

            var table = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == restaurantId);
            if (table == null)
            {
                return Json(new { success = false, message = "Table not found." });
            }

            table.Status = status;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Table status changed to {status}" });
        }
    }
}
