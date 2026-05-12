using BIZFLOW.Web.Models;
using BIZFLOW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BIZFLOW.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _authService;

        public HomeController(ILogger<HomeController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            // Приклад отримання поточного користувача
            var currentUser = await _authService.GetCurrentUserAsync(HttpContext);
            if (currentUser != null)
            {
                ViewBag.UserName = currentUser.FullName ?? currentUser.UserName;
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
