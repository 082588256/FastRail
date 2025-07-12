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
        private readonly IPaymentService _paymentService;
        private readonly IEmailService _emailService;
        private readonly ITripService _tripService;
        private readonly IPricingService _pricingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            IBookingService bookingService,
            IPaymentService paymentService,
            IEmailService emailService,
            ITripService tripService,
            IPricingService pricingService,
            ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
            _emailService = emailService;
            _tripService = tripService;
            _pricingService = pricingService;
            _logger = logger;
        }
        [HttpPost("guest-lookup")]
        public async Task<ActionResult> LookupGuestBooking( GuestBookingLookupRequest request)
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


        [HttpPost("create-temporary")]
        public async Task<ActionResult<ApiResponse<CreateBookingResponse>>> CreateTemporaryBooking([FromForm] CreateBookingRequest request)
        {
            try
            {
                _logger.LogInformation("Creating {BookingType} booking for trip {TripId}, seat {SeatId}",
                    request.IsGuestBooking ? "guest" : "user",
                    request.TripId, request.SeatId);
                if (request.TripId <= 0 || request.SeatId <= 0)
                {
                    return BadRequest(new ApiResponse<CreateBookingResponse>
                    {
                        Success = false,
                        Message = "Thông tin chuyến tàu và ghế không hợp lệ",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }
                if (!request.IsGuestBooking && (!request.UserId.HasValue || request.UserId <= 0))
                {
                    return BadRequest(new ApiResponse<CreateBookingResponse>
                    {
                        Success = false,
                        Message = "User ID không hợp lệ cho user booking",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Kiểm tra chuyến tàu có tồn tại và còn hoạt động
                //var tripExists = await _tripService.IsTripActiveAsync(request.TripId);
                //if (!tripExists)
                //{
                //    return BadRequest(new ApiResponse<CreateBookingResponse>
                //    {
                //        Success = false,
                //        Message = "Chuyến tàu không tồn tại hoặc đã ngưng hoạt động",
                //        RequestId = HttpContext.TraceIdentifier
                //    });
                //}

                //// Kiểm tra ghế có available không
                //var seatAvailable = await _tripService.IsSeatAvailableAsync(request.TripId, request.SeatId);
                //if (!seatAvailable)
                //{
                //    return BadRequest(new ApiResponse<CreateBookingResponse>
                //    {
                //        Success = false,
                //        Message = "Ghế đã có người đặt hoặc không tồn tại",
                //        RequestId = HttpContext.TraceIdentifier
                //    });
                //}
                // Tạo booking
                var result = await _bookingService.CreateTemporaryBookingAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("Successfully created {BookingType} booking {BookingId} with code {BookingCode}",
                        request.IsGuestBooking ? "guest" : "user",
                        result.BookingId,
                        result.BookingCode);

                    var responseMessage = request.IsGuestBooking
                        ? "Đặt chỗ thành công! Vui lòng lưu mã booking và hoàn tất thanh toán trong 5 phút."
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
                _logger.LogError(ex, "Error creating {BookingType} booking",
                    request.IsGuestBooking ? "guest" : "user");
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