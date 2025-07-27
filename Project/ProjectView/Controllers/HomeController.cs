using Microsoft.AspNetCore.Mvc;
using ProjectView.Models;
using ProjectView.Services;
using System.Diagnostics;

namespace ProjectView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISearchService _bookingService;
        private readonly IBasicDataService _basicDataService;
        public HomeController(ISearchService bookingService, IBasicDataService basicDataService)
        {
            _bookingService = bookingService;
            _basicDataService = basicDataService;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.HideNavbar = true;
            var stations = await _basicDataService.GetStationSelectListAsync();
            ViewBag.Stations = await _basicDataService.GetStationSelectListAsync();
            ViewBag.Trains = await _basicDataService.GetTrainSelectListAsync();
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult BookRide()
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

