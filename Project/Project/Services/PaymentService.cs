//using Microsoft.AspNetCore.Mvc;

//using Project.Services;
//using Project; // Thêm namespace cho EmailService

//namespace ProjectView.Controllers
//{
//    public class PaymentController : Controller
//    {
//        private readonly FastRailDbContext _context;
//        private readonly IEmailService _emailService;
//        private readonly ILogger<PaymentController> _logger;

//        public PaymentController(
//            FastRailDbContext context,
//            IEmailService emailService,
//            ILogger<PaymentController> logger)
//        {
//            _context = context;
//            _emailService = emailService;
//            _logger = logger;
//        }

//        [HttpGet]
//        public IActionResult Index(int bookingId)
//        {
//            ViewBag.HideNavbar = true;
//            ViewBag.BookingId = bookingId;
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> Result(int bookingId, string status)
//        {
//            var booking = await _context.Bookings.FindAsync(bookingId);
//            if (booking == null) return NotFound();

//            try
//            {
//                if (status == "Success")
//                {
//                    booking.PaymentStatus = "Completed";
//                    await _context.SaveChangesAsync();

//                    // Gửi email xác nhận thanh toán
//                    await _emailService.SendPaymentConfirmationAsync(
//                        booking.PassengerEmail,  // Email người nhận
//                        booking.BookingCode,     // Mã booking
//                        booking.TotalPrice       // Tổng tiền
//                    );

//                    _logger.LogInformation("Payment confirmed and email sent for booking {BookingId}", bookingId);
//                    return RedirectToAction("Success", "Booking", new { bookingId });
//                }
//                else
//                {
//                    booking.PaymentStatus = "Failed";
//                    await _context.SaveChangesAsync();
//                    return RedirectToAction("Fail", "Booking", new { bookingId });
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error processing payment for booking {BookingId}", bookingId);
//                return RedirectToAction("Error", "Home", new { message = "Có lỗi xảy ra khi xử lý thanh toán" });
//            }
//        }
//    }
//}