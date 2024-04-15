using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleSocialMediaPlatform.Models;
using System.Diagnostics;

namespace SimpleSocialMediaPlatform.Controllers
{
   
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            ViewData["UserID"] = userId;
            ViewData["UserID"] = _userManager.GetUserId(this.User);

            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Assuming you want the username
                ViewData["UserName"] = user.UserName;

                // If you have a full name property and prefer to use it if available
                ViewData["UserFullName"] = string.IsNullOrWhiteSpace(user.FullName) ? user.UserName : user.FullName;
            }
            else
            {
                ViewData["UserName"] = "Guest";
                ViewData["UserFullName"] = "Guest";
            }

            return View();
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
}
