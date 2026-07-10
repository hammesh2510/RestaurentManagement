using Microsoft.AspNetCore.Authorization;
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

        public TableController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Table
        public async Task<IActionResult> Index()
        {
            var tables = await _context.RestaurantTables.ToListAsync();
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
            if (ModelState.IsValid)
            {
                // Check if table number is unique
                var exists = await _context.RestaurantTables.AnyAsync(t => t.TableNumber == table.TableNumber);
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
            var table = await _context.RestaurantTables.FindAsync(id);
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

            if (ModelState.IsValid)
            {
                var exists = await _context.RestaurantTables.AnyAsync(t => t.TableNumber == table.TableNumber && t.Id != table.Id);
                if (exists)
                {
                    ModelState.AddModelError("TableNumber", "A table with this number already exists.");
                    return View(table);
                }

                try
                {
                    _context.Update(table);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Table {table.TableNumber} updated successfully!";
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
            var table = await _context.RestaurantTables.FindAsync(id);
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
            var table = await _context.RestaurantTables.FindAsync(id);
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
