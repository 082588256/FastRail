using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISearchService _bookingService;
        private readonly IBasicDataService _basicDataService;
        public HomeController(ISearchService bookingService, IBasicDataService basicDataService)
        {
            _bookingService = bookingService;
            _basicDataService = basicDataService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stations = await _basicDataService.GetStationSelectListAsync();
            ViewBag.Stations = await _basicDataService.GetStationSelectListAsync();
            ViewBag.Trains = await _basicDataService.GetTrainSelectListAsync();
            return View("~/Views/Home/Index.cshtml");
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
        public IActionResult seat()
        {
            return View();
        }
        public IActionResult taxi()
        {
            return View();
        }
        public IActionResult singleTaxi()
        {
            return View();
        }
        public IActionResult driver()
        {
            return View();
         }
        

    }
}
