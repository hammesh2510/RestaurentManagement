using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;

            var model = new LoginViewModel();
            if (Request.Cookies.TryGetValue("RememberMeEmail", out var email))
            {
                model.Email = email;
                model.RememberMe = true;
            }
            if (Request.Cookies.TryGetValue("RememberMePassword", out var pwd))
            {
                model.Password = pwd;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Find user to verify if they are active
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact administration.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (model.RememberMe)
                    {
                        var cookieOptions = new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(14),
                            HttpOnly = true,
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Lax
                        };
                        Response.Cookies.Append("RememberMeEmail", model.Email, cookieOptions);
                        Response.Cookies.Append("RememberMePassword", model.Password, cookieOptions);
                    }
                    else
                    {
                        Response.Cookies.Delete("RememberMeEmail");
                        Response.Cookies.Delete("RememberMePassword");
                    }

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            ViewBag.Managers = managers.Select(m => new { m.Id, m.FullName, m.Email }).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Role != "Manager" && string.IsNullOrEmpty(model.SelectedManagerId))
                {
                    ModelState.AddModelError("SelectedManagerId", "Please select a Manager / Restaurant Location.");
                    var managersList = await _userManager.GetUsersInRoleAsync("Manager");
                    ViewBag.Managers = managersList.Select(m => new { m.Id, m.FullName, m.Email }).ToList();
                    return View(model);
                }

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email address is already in use.");
                    var managersList = await _userManager.GetUsersInRoleAsync("Manager");
                    ViewBag.Managers = managersList.Select(m => new { m.Id, m.FullName, m.Email }).ToList();
                    return View(model);
                }

                string[] validRoles = { "Manager", "Cashier", "Waiter", "Chef" };
                if (!validRoles.Contains(model.Role))
                {
                    ModelState.AddModelError("Role", "Invalid role selected.");
                    var managersList = await _userManager.GetUsersInRoleAsync("Manager");
                    ViewBag.Managers = managersList.Select(m => new { m.Id, m.FullName, m.Email }).ToList();
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                user.RestaurantId = model.Role == "Manager" ? user.Id : model.SelectedManagerId!;

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    }
                    await _userManager.AddToRoleAsync(user, model.Role);

                    var names = model.FullName.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var firstName = names.Length > 0 ? names[0] : model.FullName;
                    var lastName = names.Length > 1 ? names[1] : "Employee";

                    var employee = new Employee
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = model.Email,
                        Phone = model.Phone,
                        Salary = 0.00m,
                        HireDate = DateTime.UtcNow,
                        IsActive = true,
                        UserId = user.Id
                    };

                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["SuccessMessage"] = $"Registration successful! Welcome, {model.FullName}.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var fallbackManagers = await _userManager.GetUsersInRoleAsync("Manager");
            ViewBag.Managers = fallbackManagers.Select(m => new { m.Id, m.FullName, m.Email }).ToList();
            return View(model);
        }
    }
}
