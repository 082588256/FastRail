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
        private readonly IQRService _qrService;

        public BookingController(
            IBookingService bookingService,
            IPaymentService paymentService,
            IEmailService emailService,
            ITripService tripService,
            IPricingService pricingService,
            ILogger<BookingController> logger,
            IQRService qrService)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
            _emailService = emailService;
            _tripService = tripService;
            _pricingService = pricingService;
            _logger = logger;
            _qrService = qrService;
        }
        [HttpPost("guest-lookup")]
        public async Task<ActionResult> LookupGuestBooking( GuestBookingLookupRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.BookingCode))
                {
                    return BadRequest(new ApiResponse<TicketDetailsResponse>
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
                    return NotFound(new ApiResponse<TicketDetailsResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy booking hoặc thông tin tra cứu không đúng",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                return Ok(new ApiResponse<TicketDetailsResponse>
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
                return StatusCode(500, new ApiResponse<TicketDetailsResponse>
                {
                    Success = false,
                    Message = "Lỗi hệ thống",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
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


                    // Generate QR from TicketCode (assume TicketCode is returned in result)
                    var qrData = await _qrService.GenerateQRCodeAsync(result.TicketCode);

                    return Ok(new ApiResponse<CreateBookingResponse>
                    {
                        Success = true,
                        Data = result,
                        Message = "Booking and ticket created successfully!",
                        RequestId = HttpContext.TraceIdentifier,
                        Errors = null
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