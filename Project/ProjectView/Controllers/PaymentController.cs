using Microsoft.AspNetCore.Mvc;
using ProjectView.Models; 

namespace ProjectView.Controllers
{
    public class PaymentController : Controller
    {
        private readonly TrainBookingSystemContext _context;
        public PaymentController(TrainBookingSystemContext context)
        {
            _context = context;
        }

        // Hiển thị trang chọn thành công/thất bại
        [HttpGet]
        public IActionResult Index(int bookingId)
        {
            ViewBag.HideNavbar = true;
            ViewBag.BookingId = bookingId;
            return View();
        }

        // Xử lý kết quả thanh toán
        [HttpPost]
        public async Task<IActionResult> Result(int bookingId, string status)
        {
            var booking = await _context.Booking.FindAsync(bookingId);
            if (booking == null) return NotFound();

            if (status == "Success")
            {
                booking.PaymentStatus = "Completed";
                await _context.SaveChangesAsync();
                return RedirectToAction("Success", "Booking", new { bookingId });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}