using BIZFLOW.Web.Models.ViewModels;
using BIZFLOW.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BIZFLOW.Web.Controllers
{
    // Controller for user authentication (login, register, logout)
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        // Constructor with authentication service injection
        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: Account/Login
        // Display login form
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        // Process login form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Attempt to login with provided credentials
            var user = await _authService.LoginAsync(model.UserName, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Incorrect username or password");
                return View(model);
            }

            // Save user ID in session for authentication
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.UserName);

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        // Display registration form
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        // Process registration form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Try to register new user
            var result = await _authService.RegisterAsync(model.UserName, model.Password, model.FullName);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            // Automatically login after successful registration
            var user = await _authService.LoginAsync(model.UserName, model.Password);
            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.UserName);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Logout
        // Handle user logout
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync(HttpContext);
            return RedirectToAction("Login");
        }
    }
}
