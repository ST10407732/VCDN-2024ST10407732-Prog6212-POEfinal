using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProgPoePart2_6212.Models;
using System.Diagnostics;

namespace ProgPoePart2_6212.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger,UserManager<User> userManager)
        {
            _logger = logger;
            _userManager = userManager;

        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // This gets the currently logged-in user.

            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user); // Get the roles assigned to the user.

                ViewData["isCoordinator"] = roles.Contains("Coordinator");
                ViewData["isManager"] = roles.Contains("Manager");
                ViewData["isLecturer"] = roles.Contains("Lecturer");
                ViewData["isHR"] = roles.Contains("HR");
            }

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
