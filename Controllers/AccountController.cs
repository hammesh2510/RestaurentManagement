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
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Role != "Manager")
            {
                if (string.IsNullOrEmpty(model.SelectedManagerId))
                {
                    ModelState.AddModelError("SelectedManagerId", "Manager's Referral Email is required.");
                }
                else
                {
                    var manager = await _userManager.FindByEmailAsync(model.SelectedManagerId);
                    if (manager == null || !await _userManager.IsInRoleAsync(manager, "Manager"))
                    {
                        ModelState.AddModelError("SelectedManagerId", "A manager with this email address was not found.");
                    }
                    else
                    {
                        // Map SelectedManagerId to manager's database Id
                        model.SelectedManagerId = manager.Id;
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email address is already in use.");
                    return View(model);
                }

                string[] validRoles = { "Manager", "Cashier", "Waiter", "Chef" };
                if (!validRoles.Contains(model.Role))
                {
                    ModelState.AddModelError("Role", "Invalid role selected.");
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

            return View(model);
        }

        // GET: Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Login");
            }

            var userObj = await _userManager.GetUserAsync(User);
            if (userObj == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(userObj);
            var userRole = roles.FirstOrDefault() ?? "Staff";

            // Find matching employee details
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userObj.Id);

            var model = new ProfileViewModel
            {
                Email = userObj.Email!,
                FullName = userObj.FullName,
                Phone = employee?.Phone ?? userObj.PhoneNumber ?? string.Empty,
                Role = userRole
            };

            return View(model);
        }

        // POST: Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Login");
            }

            var userObj = await _userManager.GetUserAsync(User);
            if (userObj == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(userObj);
            model.Role = roles.FirstOrDefault() ?? "Staff";
            model.Email = userObj.Email!; // Prevent tampering with email

            if (ModelState.IsValid)
            {
                userObj.FullName = model.FullName;
                userObj.PhoneNumber = model.Phone;

                var result = await _userManager.UpdateAsync(userObj);
                if (result.Succeeded)
                {
                    // Also update linked Employee entity
                    var employee = _context.Employees.FirstOrDefault(e => e.UserId == userObj.Id);
                    if (employee != null)
                    {
                        var names = model.FullName.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        employee.FirstName = names.Length > 0 ? names[0] : model.FullName;
                        employee.LastName = names.Length > 1 ? names[1] : "Employee";
                        employee.Phone = model.Phone;

                        _context.Employees.Update(employee);
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}
