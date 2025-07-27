using Microsoft.AspNetCore.Mvc;
using ProjectView.Services;

public class SearchController : Controller
{
    private readonly ISearchService _bookingService;
    private readonly IBasicDataService _basicDataService;   
    public SearchController(ISearchService bookingService,IBasicDataService basicDataService)
    {
        _bookingService = bookingService;
        _basicDataService = basicDataService;
    }

    [HttpPost]
    public async Task<IActionResult> Search(string bookingCode)
    {
        var ticket = await _bookingService.GetTicketByBookingCodeAsync(bookingCode);
        if (ticket == null)
        {
            ViewBag.Error = "Không tìm thấy vé có ID: " + bookingCode;
            return View("Search", null);
        }
        return View("Search", ticket);
    }
    [HttpGet]
    public async Task<IActionResult> index()
    {
        var stations = await _basicDataService.GetStationSelectListAsync();
        ViewBag.Stations = stations;

        return View("~/Views/Home/Index.cshtml"); 
    }
}
