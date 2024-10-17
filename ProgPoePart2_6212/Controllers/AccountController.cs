using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProgPoePart2_6212.Models;

using Microsoft.AspNetCore.Mvc;


using System.Threading.Tasks;
using ProgPoePart2_6212.ViewModels;

namespace ProgPoePart2_6212.Controllers
{

    namespace YourProjectNamespace.Controllers
    {
        public class AccountController : Controller
        {
            private readonly UserManager<User> _userManager;
            private readonly SignInManager<User> _signInManager;

            public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
            {
                _userManager = userManager;
                _signInManager = signInManager;
            }

            // Registration Action
            [HttpGet]
            public IActionResult Register()
            {
                return View();
            }

            [HttpPost]
            public async Task<IActionResult> Register(RegisterViewModel model)
            {
                if (ModelState.IsValid)
                {
                    // Check if user already exists
                    var userExists = await _userManager.FindByEmailAsync(model.Email);
                    if (userExists != null)
                    {
                        ModelState.AddModelError("Email", "User with this email already exists. Please use a different email.");
                        return View(model);
                    }

                    var user = new User
                    {
                        FullName = model.FullName,
                        UserName = model.Email,
                        Email = model.Email
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // Automatically assign "Lecturer" role to new users
                        await _userManager.AddToRoleAsync(user, "Lecturer");

                        // Use TempData to store the success message
                        TempData["RegisterSuccess"] = "Registration successful! Please log in.";
                        return RedirectToAction("Login", "Account");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                return View(model);
            }

            // Login Action
            [HttpGet]
            public IActionResult Login()
            {
                return View();
            }

            [HttpPost]
            public async Task<IActionResult> Login(LoginViewModel model)
            {
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }

                return View(model);
            }

            // Logout Action
            [HttpPost]
            public async Task<IActionResult> Logout()
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }
            public IActionResult AccessDenied()
            {
                return View();
            }
        }
    }
}