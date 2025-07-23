using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Services;
using Project.Services.Route;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly Services.ITripService _tripService;
        private readonly IBookingService _bookingService;
        private readonly IRouteService _routeService;

        public AdminController(ITripService tripService, IBookingService bookingService, IRouteService routeService)
        {
            _tripService = tripService;
            _bookingService = bookingService;
            _routeService = routeService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = new DashboardStatsResponse
            {
                TicketsToday = await _bookingService.CountTicketsTodayAsync(),
                PassengersOnBoard = await _tripService.CountPassengersOnBoardAsync(),
                ActiveRoutes = await _routeService.CountActiveRoutesAsync(),
                TodayRevenue = await _bookingService.CalculateTodayRevenueAsync(),
                UpcomingTrips = await _tripService.CountUpcomingTripsWithinHourAsync(1),
                EmptySeats = await _tripService.CountEmptySeatsTodayAsync(),
                SeatOccupancyRate = await _tripService.CalculateOccupancyRateTodayAsync()
            };

            return Ok(stats);
        }
    }

    internal class DashboardStatsResponse
    {
        public int TicketsToday { get; set; }
        public int PassengersOnBoard { get; set; }
        public object ActiveRoutes { get; set; }
        public object TodayRevenue { get; set; }
        public object UpcomingTrips { get; set; }
        public object EmptySeats { get; set; }
        public object SeatOccupancyRate { get; set; }
    }