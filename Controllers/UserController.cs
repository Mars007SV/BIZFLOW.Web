using BIZFLOW.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIZFLOW.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly BizFlowDbContext _context;

        public UserController(BizFlowDbContext context)
        {
            _context = context;
        }

        // Показуємо інформацію про поточного користувача
        public async Task<IActionResult> Profile()
        {
            var deviceId = Request.Cookies["DeviceId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.DeviceId == deviceId);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        // Оновлюємо ім'я користувача
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string userName)
        {
            var deviceId = Request.Cookies["DeviceId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.DeviceId == deviceId);
            if (user != null)
            {
                user.UserName = userName;
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
            var deviceId = Request.Cookies["DeviceId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                return Json(new { success = false, message = "No user found" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.DeviceId == deviceId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            return Json(new
            {
                success = true,
                user = new
                {
                    user.Id,
                    user.UserName,
                    user.CreatedAt,
                    user.LastAccessAt
                }
            });
        }
    }
}
