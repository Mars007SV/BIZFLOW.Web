using BIZFLOW.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BIZFLOW.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Product { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
