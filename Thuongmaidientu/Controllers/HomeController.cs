using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Thuongmaidientu.Models;

namespace Thuongmaidientu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/404")]
        public IActionResult PageNotFound()
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Indes()
        {
            ViewData["MetaDescription"] = "N&NShop cung c?p ða d?ng s?n ph?m týõi ngon, ch?t lý?ng cao v?i giá c? h?p l?. Khám phá ngay các s?n ph?m n?i b?t và ýu ð?i h?p d?n!";
            return View();
        }

    }
}
