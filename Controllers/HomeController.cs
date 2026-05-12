using BIZFLOW.Web.Models;
using BIZFLOW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BIZFLOW.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            // Приклад отримання поточного користувача
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext);
            if (currentUser != null)
            {
                ViewBag.UserName = currentUser.UserName;
                ViewBag.UserId = currentUser.Id;
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
