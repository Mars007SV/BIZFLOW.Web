using BIZFLOW.Web.Data;
using BIZFLOW.Web.Models;
using BIZFLOW.Web.Models.ViewModels;
using BIZFLOW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BIZFLOW.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _authService;
        private readonly BizFlowDbContext _context;

        public HomeController(ILogger<HomeController> logger, IAuthService authService, BizFlowDbContext context)
        {
            _logger = logger;
            _authService = authService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Отримання поточного користувача
            var currentUser = await _authService.GetCurrentUserAsync(HttpContext);
            if (currentUser != null)
            {
                ViewBag.UserName = currentUser.FullName ?? currentUser.UserName;
                ViewBag.UserId = currentUser.Id;
            }

            // Збір статистики
            var viewModel = new DashboardViewModel
            {
                Statistics = new DashboardStatistics
                {
                    TotalProducts = await _context.Products.CountAsync(),
                    ProductsInDeficit = await _context.Products.CountAsync(p => p.Quantity < 5),
                    RecentOperationsCount = await _context.Operations
                        .Where(o => o.Date >= DateTime.Now.AddDays(-7))
                        .CountAsync(),
                    TotalCategories = await _context.Categories.CountAsync()
                },
                RecentActivities = await _context.Operations
                    .Include(o => o.Product)
                    .OrderByDescending(o => o.Date)
                    .Take(10)
                    .Select(o => new RecentActivity
                    {
                        ProductName = o.Product!.Name,
                        OperationType = o.Type,
                        Quantity = o.Quantity,
                        UnitOfMeasure = o.Product!.UnitOfMeasure,
                        Date = o.Date,
                        UserName = o.UserName
                    })
                    .ToListAsync()
            };

            return View(viewModel);
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
