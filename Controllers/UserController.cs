using BIZFLOW.Web.Data;
using BIZFLOW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIZFLOW.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly BizFlowDbContext _context;
        private readonly IAuthService _authService;

        public UserController(BizFlowDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // Показуємо інформацію про поточного користувача
        public async Task<IActionResult> Profile()
        {
            var user = await _authService.GetCurrentUserAsync(HttpContext);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // Оновлюємо інформацію користувача
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName)
        {
            var user = await _authService.GetCurrentUserAsync(HttpContext);
            if (user != null)
            {
                user.FullName = fullName;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Profile");
        }

        // Показуємо всіх користувачів (для адміністрування)
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.OrderByDescending(u => u.LastAccessAt).ToListAsync();
            return View(users);
        }

        // Отримуємо поточного користувача (для API)
        [HttpGet]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _authService.GetCurrentUserAsync(HttpContext);
            if (user == null)
            {
                return Json(new { success = false, message = "No user found" });
            }

            return Json(new
            {
                success = true,
                user = new
                {
                    user.Id,
                    user.UserName,
                    user.FullName,
                    user.CreatedAt,
                    user.LastAccessAt
                }
            });
        }
    }
}
