using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class BookingController : Controller
    {
        public readonly TrainBookingSystemContext _context;
        private readonly ITripService _tripService;
        private readonly ISeatService _seatService;
        public BookingController(ITripService tripService, ISeatService seatService, TrainBookingSystemContext context)
        {
            _tripService = tripService;
            _seatService = seatService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> TrainView(TripSearchRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Thông tin tìm kiếm không hợp lệ.");
            }

            var trips = await _tripService.SearchTripsAsync(request);

            return View(trips); // Trả về View chứa danh sách chuyến tàu
        }
        [HttpPost]
        public IActionResult SelectSeat(TripDetailViewModel trip)
        {

            int tripId = trip.TripId;
            string fromStation = trip.DepartureStation;
            string toStation = trip.ArrivalStation;

            
            return RedirectToAction("SelectSeat", new
            {
                tripId,
                fromStation,
                toStation
            });
        }
        [HttpGet]
        public async Task<IActionResult> SelectSeat(int tripId, string fromStation, string toStation)
        {
            var fromId = await _context.Stations
                .Where(s => s.StationName == fromStation)
                .Select(s => s.StationId)
                .FirstOrDefaultAsync();

            var toId = await _context.Stations
                .Where(s => s.StationName == toStation)
                .Select(s => s.StationId)
                .FirstOrDefaultAsync();

            var seats = await _seatService.GetAvailableSeatsAsync(tripId, fromId, toId);

            ViewBag.Stations = JsonConvert.SerializeObject(_context.Stations); // Truyền xuống view dưới dạng JSON
            ViewBag.TripId = tripId;
            ViewBag.FromStation = fromStation;
            ViewBag.ToStation = toStation;

            //  Gán thêm các ID để dùng trong JavaScript
            ViewBag.DepartureStationId = fromId;
            ViewBag.ArrivalStationId = toId;

            return View(seats);
        }
        public IActionResult bookRide()
        {
            return View();
        }
        public async Task<IActionResult> Success(int bookingId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound("Không tìm thấy booking");
            }

            var tickets = await _context.Tickets
                .Where(t => t.BookingId == bookingId)
                .ToListAsync();

            var viewModel = new BookingSuccessViewModel
            {
                Booking = booking,
                Tickets = tickets
            };

            return View(viewModel);
        }
    }
}

    


