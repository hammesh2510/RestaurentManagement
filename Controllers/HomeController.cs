using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole("Manager"))
        {
            return RedirectToAction("Dashboard", "Manager");
        }
        return RedirectToAction("OrderBoard", "Order");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
