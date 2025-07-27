using Microsoft.AspNetCore.Mvc;
using Project.DTO;
using Project.DTOs;
using Project.Models;
using Project.Services;


namespace Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        
        private readonly IEmailService _emailService;
        private readonly ITripService _tripService;
        private readonly IPricingService _pricingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            IBookingService bookingService,
            
            IEmailService emailService,
            ITripService tripService,
            IPricingService pricingService,
            ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            
            _emailService = emailService;
            _tripService = tripService;
            _pricingService = pricingService;
            _logger = logger;
        }
        [HttpPost("guest-lookup")]
        public async Task<ActionResult> LookupGuestBooking(GuestBookingLookupRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.BookingCode))
                {
                    return BadRequest(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "Vui lòng nhập mã booking",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                var booking = await _bookingService.LookupGuestBookingAsync(request);
                _logger.LogInformation("Bắt đầu tra cứu booking: {BookingCode}", request.BookingCode);

                if (booking == null)
                {
                    return NotFound(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy booking hoặc thông tin tra cứu không đúng",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                return Ok(new ApiResponse<BookingDetailsResponse>
                {
                    Success = true,
                    Data = booking,
                    Message = "Tìm thấy booking",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up guest booking with code {BookingCode}", request.BookingCode);
                return StatusCode(500, new ApiResponse<BookingDetailsResponse>
                {
                    Success = false,
                    Message = "Lỗi hệ thống",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelBooking(int id)
        {
            var result = await _bookingService.CancelBookingAsync(id);
            if (!result)
                return BadRequest("Không thể hủy booking này");
            return Ok();
        }
        [HttpPost("{id}/confirm")]
        public async Task<ActionResult> ConfirmBooking(int id, [FromQuery] string? transactionId = null)
        {
            var result = await _bookingService.ConfirmBookingAsync(id, transactionId);
            if (!result)
                return BadRequest("Không thể xác nhận booking này");

            // Lấy thông tin booking để gửi email
            var booking = await _bookingService.GetBookingDetailsAsync(id);
            if (booking != null)
            {
                await _emailService.SendPaymentConfirmationAsync(
                    booking.PassengerEmail,
                    booking.BookingCode,
                    booking.TotalPrice
                );
            }

            return Ok();
        }
        [HttpPost("create-temporary")]
        public async Task<ActionResult<ApiResponse<CreateBookingResponse>>> CreateTemporaryBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                _logger.LogInformation("Creating {BookingType} booking for trip {TripId}",
                    request.IsGuestBooking ? "guest" : "user", request.TripId);

                if (request.TripId <= 0)
                {
                    return BadRequest(new ApiResponse<CreateBookingResponse>
                    {
                        Success = false,
                        Message = "TripId không hợp lệ",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                if (!request.IsGuestBooking && (!request.UserId.HasValue || request.UserId <= 0))
                {
                    return BadRequest(new ApiResponse<CreateBookingResponse>
                    {
                        Success = false,
                        Message = "UserId không hợp lệ cho user booking",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                if (request.Tickets == null || !request.Tickets.Any())
                {
                    return BadRequest(new ApiResponse<CreateBookingResponse>
                    {
                        Success = false,
                        Message = "Danh sách vé không được để trống",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                var result = await _bookingService.CreateTemporaryBookingAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("Successfully created {BookingType} booking {BookingId} with code {BookingCode}",
                        request.IsGuestBooking ? "guest" : "user",
                        result.BookingId,
                        result.BookingCode);

                    var responseMessage = request.IsGuestBooking
                        ? "Đặt chỗ thành công! Vui lòng lưu mã booking để tra cứu và hoàn tất thanh toán trong 5 phút."
                        : "Đặt chỗ thành công! Vui lòng hoàn tất thanh toán trong 5 phút.";

                    return Ok(new ApiResponse<CreateBookingResponse>
                    {
                        Success = true,
                        Data = result,
                        Message = responseMessage,
                        RequestId = HttpContext.TraceIdentifier
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<CreateBookingResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        RequestId = HttpContext.TraceIdentifier
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {BookingType} booking", request.IsGuestBooking ? "guest" : "user");
                return StatusCode(500, new ApiResponse<CreateBookingResponse>
                {
                    Success = false,
                    Message = "Lỗi hệ thống khi tạo booking. Vui lòng thử lại sau.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

    }
}