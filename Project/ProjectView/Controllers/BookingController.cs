using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProjectView.Models;
using ProjectView.Services;

namespace ProjectView.Controllers
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
            ViewBag.HideNavbar = true;
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
            ViewBag.HideNavbar = true;
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
            ViewBag.HideNavbar = true;
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
            try
            {
                ViewBag.HideNavbar = true;

                // Log trước khi query
                Console.WriteLine($"Querying booking with ID: {bookingId}");

                // Thay đổi cách query để xử lý null safety
                var booking = await _context.Booking
                    .Include(b => b.Ticket)  // Include relationship
                    .AsSplitQuery()  // Tách thành multiple queries để tránh Cartesian explosion
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    Console.WriteLine($"Booking {bookingId} not found");
                    return NotFound($"Không tìm thấy booking với ID: {bookingId}");
                }

                Console.WriteLine($"Found booking: {booking.BookingCode}");

                // Query tickets với null check
                var tickets = await _context.Ticket
                    .Where(t => t.BookingId == bookingId)
                    .AsNoTracking()
                    .Select(t => new Ticket
                    {
                        TicketId = t.TicketId,
                        BookingId = t.BookingId,
                        TicketCode = t.TicketCode ?? string.Empty,
                        PassengerName = t.PassengerName ?? string.Empty,
                        PassengerPhone = t.PassengerPhone,
                        PassengerIdCard = t.PassengerIdCard,
                        Status = t.Status ?? "Unknown",
                        TotalPrice=t.TotalPrice
                        // Thêm các properties khác với null check
                    })
                    .ToListAsync();

                Console.WriteLine($"Found {tickets.Count} tickets");

                var viewModel = new BookingSuccessViewModel
                {
                    Booking = booking,
                    Tickets = tickets
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log chi tiết error
                Console.WriteLine($"Error in Success action: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}

    


