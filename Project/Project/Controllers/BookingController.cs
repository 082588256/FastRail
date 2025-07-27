using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IQRService _qrService;
        private readonly IAuthService _authService;
        private readonly FastRailDbContext _context;

        public BookingController(
            IBookingService bookingService,
            
            IEmailService emailService,
            ITripService tripService,
            IPricingService pricingService,
            ILogger<BookingController> logger,
            IQRService qrService,
            IAuthService authService,
            FastRailDbContext context)
        {
            _bookingService = bookingService;
            
            _emailService = emailService;
            _tripService = tripService;
            _pricingService = pricingService;
            _logger = logger;
            _qrService = qrService;
            _authService = authService;
            _context = context;
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

                return Ok(new ApiResponse<Project.DTOs.BookingDetailsResponse>
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
                return StatusCode(500, new ApiResponse<DTOs.BookingDetailsResponse>
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
        [HttpGet("staff/customers")]
        public async Task<ActionResult<ApiResponse<PagedResponse<CustomerBookingResponse>>>> GetCustomerBookings(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? paymentStatus = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] int? tripId = null,
            [FromHeader(Name = "Authorization")] string? authorization = null)
        {
            try
            {
                // Validate STAFF token
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return Unauthorized(new ApiResponse<PagedResponse<CustomerBookingResponse>>
                    {
                        Success = false,
                        Message = "Authorization header is required",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                var token = authorization.Substring("Bearer ".Length);
                var isValid = await _authService.ValidateStaffTokenAsync(token);
                if (!isValid)
                {
                    return Unauthorized(new ApiResponse<PagedResponse<CustomerBookingResponse>>
                    {
                        Success = false,
                        Message = "Invalid or expired token",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Build query
                var query = _context.Bookings
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DepartureStation)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.ArrivalStation)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Train)
                    .Include(b => b.User)
                    .Include(b => b.Tickets)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(b =>
                        b.BookingCode.ToLower().Contains(searchTerm) ||
                        (b.ContactName != null && b.ContactName.ToLower().Contains(searchTerm)) ||
                        (b.ContactPhone != null && b.ContactPhone.ToLower().Contains(searchTerm)) ||
                        (b.User != null && b.User.FullName.ToLower().Contains(searchTerm)) ||
                        (b.User != null && b.User.Phone != null && b.User.Phone.ToLower().Contains(searchTerm))
                    );
                }

                // Apply payment status filter
                if (!string.IsNullOrWhiteSpace(paymentStatus))
                {
                    query = query.Where(b => b.PaymentStatus == paymentStatus);
                }

                // Apply date range filter
                if (createdFrom.HasValue)
                {
                    query = query.Where(b => b.CreatedAt >= createdFrom.Value);
                }
                if (createdTo.HasValue)
                {
                    query = query.Where(b => b.CreatedAt <= createdTo.Value);
                }

                // Apply trip filter
                if (tripId.HasValue)
                {
                    query = query.Where(b => b.TripId == tripId.Value);
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var skip = (page - 1) * pageSize;
                var bookings = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to response
                var customerBookings = bookings.Select(b => new CustomerBookingResponse
                {
                    BookingId = b.BookingId,
                    BookingCode = b.BookingCode,
                    BookingStatus = b.BookingStatus,
                    PaymentStatus = b.PaymentStatus,
                    CreatedAt = b.CreatedAt,
                    ConfirmedAt = b.ConfirmedAt,
                    ExpirationTime = b.ExpirationTime,
                    IsGuestBooking = b.IsGuestBooking,
                    ContactName = b.ContactName ?? b.User?.FullName ?? "N/A",
                    ContactPhone = b.ContactPhone ?? b.User?.Phone ?? "N/A",
                    ContactEmail = b.ContactEmail ?? b.User?.Email ?? "N/A",
                    TripCode = b.Trip?.TripCode,
                    TrainNumber = b.Trip?.Train?.TrainNumber,
                    DepartureStation = b.Trip?.Route?.DepartureStation?.StationName,
                    ArrivalStation = b.Trip?.Route?.ArrivalStation?.StationName,
                    DepartureTime = b.Trip?.DepartureTime,
                    ArrivalTime = b.Trip?.ArrivalTime,
                    TotalPrice = b.Tickets.Sum(t => t.FinalPrice),
                    TicketCount = b.Tickets.Count,
                    PassengerCount = b.Tickets.Count
                }).ToList();

                var pagedResponse = new PagedResponse<CustomerBookingResponse>
                {
                    Data = customerBookings,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(new ApiResponse<PagedResponse<CustomerBookingResponse>>
                {
                    Success = true,
                    Data = pagedResponse,
                    Message = "Customer bookings retrieved successfully",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer bookings");
                return StatusCode(500, new ApiResponse<PagedResponse<CustomerBookingResponse>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving customer bookings",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet("staff/trips")]
        public async Task<ActionResult<ApiResponse<List<TripOptionResponse>>>> GetAvailableTrips(
            [FromHeader(Name = "Authorization")] string? authorization = null)
        {
            try
            {
                // Validate staff token
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return Unauthorized(new ApiResponse<List<TripOptionResponse>>
                    {
                        Success = false,
                        Message = "Invalid authorization header",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                var token = authorization.Substring("Bearer ".Length);
                var isValid = await _authService.ValidateStaffTokenAsync(token);
                if (!isValid)
                {
                    return Unauthorized(new ApiResponse<List<TripOptionResponse>>
                    {
                        Success = false,
                        Message = "Invalid or expired token",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Get all active trips with bookings
                var trips = await _context.Trip
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DepartureStation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.ArrivalStation)
                    .Include(t => t.Train)
                    .Where(t => t.IsActive && _context.Bookings.Any(b => b.TripId == t.TripId))
                    .OrderByDescending(t => t.DepartureTime)
                    .Select(t => new TripOptionResponse
                    {
                        TripId = t.TripId,
                        TripCode = t.TripCode,
                        TripName = t.TripName,
                        TrainNumber = t.Train.TrainNumber,
                        DepartureStation = t.Route.DepartureStation.StationName,
                        ArrivalStation = t.Route.ArrivalStation.StationName,
                        DepartureTime = t.DepartureTime,
                        DisplayText = $"{t.TripCode} - {t.Train.TrainNumber} ({t.Route.DepartureStation.StationName} → {t.Route.ArrivalStation.StationName})"
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<TripOptionResponse>>
                {
                    Success = true,
                    Data = trips,
                    Message = "Available trips retrieved successfully",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available trips");
                return StatusCode(500, new ApiResponse<List<TripOptionResponse>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving available trips",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}