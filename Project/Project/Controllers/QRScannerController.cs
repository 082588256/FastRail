using Microsoft.AspNetCore.Mvc;
using Project.DTO;
using Project.DTOs;
using Project.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRScannerController : ControllerBase
    {
        private readonly IQRService _qrService;
        private readonly IBookingService _bookingService;
        private readonly IAuthService _authService;
        private readonly ILogger<QRScannerController> _logger;
        private readonly FastRailDbContext _context;

        public QRScannerController(
            IQRService qrService,
            IBookingService bookingService,
            IAuthService authService,
            ILogger<QRScannerController> logger,
            FastRailDbContext context)
        {
            _qrService = qrService;
            _bookingService = bookingService;
            _authService = authService;
            _logger = logger;
            _context = context;
        }

        [HttpPost("scan")]
        public async Task<ActionResult<ApiResponse<BookingDetailsResponse>>> ScanBooking(
            [FromBody] ScanBookingRequest request,
            [FromHeader(Name = "Authorization")] string? authorization)
        {
            try
            {
                // Validate STAFF token
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return Unauthorized(new ApiResponse<BookingDetailsResponse>
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
                    return Unauthorized(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "Invalid or expired token",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                if (string.IsNullOrWhiteSpace(request.QRCodeData))
                {
                    return BadRequest(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "QR code data is required",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Decode QR code to get booking code
                var bookingCode = await _qrService.DecodeQRCodeAsync(request.QRCodeData);
                if (string.IsNullOrWhiteSpace(bookingCode))
                {
                    return BadRequest(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "Invalid QR code format",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Get booking details by booking code
                var booking = await _context.Bookings
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DepartureStation)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.ArrivalStation)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Train)
                    .Include(b => b.Tickets)
                        .ThenInclude(t => t.TicketSegments)
                            .ThenInclude(ts => ts.Seat)
                    .Include(b => b.Tickets)
                        .ThenInclude(t => t.TicketSegments)
                            .ThenInclude(ts => ts.Segment)
                    .FirstOrDefaultAsync(b => b.BookingCode == bookingCode);

                if (booking == null)
                {
                    return NotFound(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "Booking not found",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Validate booking for boarding
                var validationResult = await ValidateBookingForBoarding(booking.BookingStatus, booking.Trip.DepartureTime);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = validationResult.Message,
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                _logger.LogInformation("Booking {BookingCode} scanned successfully by staff", bookingCode);

                // Map Booking to BookingDetailsResponse
                var bookingDetails = new BookingDetailsResponse
                {
                    BookingId = booking.BookingId,
                    BookingCode = booking.BookingCode,
                    BookingStatus = booking.BookingStatus,
                    PaymentStatus = booking.PaymentStatus,
                    CreatedAt = booking.CreatedAt,
                    ConfirmedAt = booking.ConfirmedAt,
                    ExpirationTime = booking.ExpirationTime,
                    IsGuestBooking = booking.IsGuestBooking,
                    ContactInfo = booking.ContactInfo,
                    ContactName = booking.ContactName,
                    ContactPhone = booking.ContactPhone,
                    ContactEmail = booking.ContactEmail,
                    TripCode = booking.Trip?.TripCode,
                    TrainNumber = booking.Trip?.Train?.TrainNumber,
                    DepartureStation = booking.Trip?.Route?.DepartureStation?.StationName,
                    ArrivalStation = booking.Trip?.Route?.ArrivalStation?.StationName,
                    DepartureTime = booking.Trip?.DepartureTime,
                    ArrivalTime = booking.Trip?.ArrivalTime,
                    TotalPrice = booking.Tickets.Sum(t => t.FinalPrice),
                    Tickets = booking.Tickets.Select(t => new TicketInfo
                    {
                        TicketCode = t.TicketCode,
                        PassengerName = t.PassengerName,
                        PassengerPhone = t.PassengerPhone,
                        Status = t.Status,
                        TotalPrice = t.TotalPrice,
                        SeatInfo = t.TicketSegments.FirstOrDefault()?.Seat != null ? 
                            $"{t.TicketSegments.FirstOrDefault()?.Seat?.Carriage?.CarriageNumber} - {t.TicketSegments.FirstOrDefault()?.Seat?.SeatNumber}" : "N/A"
                    }).ToList()
                };

                return Ok(new ApiResponse<BookingDetailsResponse>
                {
                    Success = true,
                    Data = bookingDetails,
                    Message = "Booking scanned successfully",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning booking");
                return StatusCode(500, new ApiResponse<BookingDetailsResponse>
                {
                    Success = false,
                    Message = "An error occurred while scanning booking",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponse<object>>> ValidateBooking(
            [FromBody] ValidateBookingRequest request,
            [FromHeader(Name = "Authorization")] string? authorization)
        {
            try
            {
                // Validate STAFF token
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return Unauthorized(new ApiResponse<object>
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
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid or expired token",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                if (string.IsNullOrWhiteSpace(request.BookingCode))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Booking code is required",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Get booking details by booking code
                var booking = await _context.Bookings
                    .Include(b => b.Tickets)
                    .FirstOrDefaultAsync(b => b.BookingCode == request.BookingCode);
                
                if (booking == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Booking not found",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Mark all tickets as checked in
                foreach (var ticket in booking.Tickets)
                {
                    ticket.Status = "Used";
                    ticket.CheckInTime = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Booking {BookingCode} validated and checked in by staff", request.BookingCode);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Booking validated and checked in successfully",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating booking {BookingCode}", request.BookingCode);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while validating booking",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet("generate/{bookingCode}")]
        public async Task<ActionResult> GenerateQRCode(string bookingCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bookingCode))
                {
                    return BadRequest("Booking code is required");
                }

                var qrCodeImage = await _qrService.GenerateQRCodeImageAsync(bookingCode);
                return File(qrCodeImage, "image/png", $"booking_{bookingCode}.png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for booking {BookingCode}", bookingCode);
                return StatusCode(500, "An error occurred while generating QR code");
            }
        }

        [HttpPost("scan-image")]
        public async Task<ActionResult<ApiResponse<BookingScanResponse>>> ScanBookingFromImage(
            [FromForm] ScanImageRequest request,
            [FromHeader(Name = "Authorization")] string? authorization)
        {
            // 1. Validate Authorization
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return Unauthorized(new ApiResponse<BookingScanResponse>
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
                return Unauthorized(new ApiResponse<BookingScanResponse>
                {
                    Success = false,
                    Message = "Invalid or expired token",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // 2. Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join("; ", errors));
                return BadRequest(new ApiResponse<BookingScanResponse>
                {
                    Success = false,
                    Message = "Invalid request data.",
                    Errors = errors,
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // 3. Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
            var fileName = request.QRImage.FileName?.Trim() ?? string.Empty;
            var fileExtension = Path.GetExtension(fileName).Trim().ToLowerInvariant();
            _logger.LogInformation("Uploaded file name: {FileName}, extension: {FileExtension}", fileName, fileExtension);
            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("File type not allowed: {FileName} ({FileExtension})", fileName, fileExtension);
                return BadRequest(new ApiResponse<BookingScanResponse>
                {
                    Success = false,
                    Message = $"Invalid file type: {fileExtension}. Only JPG, PNG, and BMP are allowed.",
                    Errors = new List<string> { $"File name: {fileName}", $"File extension: {fileExtension}" },
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // 4. Read image data
            byte[] imageData;
            using (var ms = new MemoryStream())
            {
                await request.QRImage.CopyToAsync(ms);
                imageData = ms.ToArray();
            }
            if (imageData.Length == 0)
            {
                return BadRequest(new ApiResponse<BookingScanResponse>
                {
                    Success = false,
                    Message = "Uploaded image is empty.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // 5. Decode QR code from image
            string bookingCode;
            try
            {
                bookingCode = await _qrService.DecodeQRCodeFromImageAsync(imageData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decoding QR code from image");
                return BadRequest(new ApiResponse<BookingScanResponse>
                {
                    Success = false,
                    Message = "Failed to decode QR code from image. Ensure the image is clear and contains a valid QR code.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            if (string.IsNullOrWhiteSpace(bookingCode))
            {
                return BadRequest(new ApiResponse<BookingScanResponse>
                {
                    Success = false,
                    Message = "No valid QR code found in the image.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // 6. Lookup booking by BookingCode
            var booking = await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Route)
                        .ThenInclude(r => r.DepartureStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Route)
                        .ThenInclude(r => r.ArrivalStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Train)
                .Include(b => b.Tickets)
                .FirstOrDefaultAsync(b => b.BookingCode == bookingCode);
            if (booking == null)
            {
                return NotFound(new ApiResponse<BookingScanResponse>
                {
                    Success = false,
                    Message = "Booking not found.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // 7. Validate booking status for boarding
            if (booking.BookingStatus != "Confirmed")
            {
                return BadRequest(new ApiResponse<BookingScanResponse>
                {
                    Success = false,
                    Message = $"Booking is not valid for boarding. Status: {booking.BookingStatus}",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            _logger.LogInformation("Booking {BookingCode} scanned from image successfully by staff", bookingCode);

            // 8. Return booking info
            var response = new BookingScanResponse
            {
                BookingCode = booking.BookingCode,
                BookingStatus = booking.BookingStatus,
                PaymentStatus = booking.PaymentStatus,
                ContactName = booking.ContactName,
                ContactPhone = booking.ContactPhone,
                TripId = booking.TripId,
                TripCode = booking.Trip?.TripCode,
                TrainNumber = booking.Trip?.Train?.TrainNumber,
                DepartureStation = booking.Trip?.Route?.DepartureStation?.StationName,
                ArrivalStation = booking.Trip?.Route?.ArrivalStation?.StationName,
                DepartureTime = booking.Trip?.DepartureTime ?? DateTime.MinValue,
                ArrivalTime = booking.Trip?.ArrivalTime,
                TotalPrice = booking.Tickets.Sum(t => t.FinalPrice),
                TicketCount = booking.Tickets.Count
            };

            return Ok(new ApiResponse<BookingScanResponse>
            {
                Success = true,
                Data = response,
                Message = "Booking scanned successfully from image.",
                RequestId = HttpContext.TraceIdentifier
            });
        }

        private async Task<(bool IsValid, string Message)> ValidateBookingForBoarding(string bookingStatus, DateTime departureTime)
        {
            // Check if booking is confirmed
            if (bookingStatus != "Confirmed")
            {
                return (false, $"Booking is not valid for boarding. Status: {bookingStatus}");
            }

            // Check if trip time is valid (within 2 hours before departure)
            var timeUntilDeparture = departureTime - DateTime.UtcNow;
            if (timeUntilDeparture.TotalHours > 2)
            {
                return (false, "Boarding is only allowed within 2 hours before departure");
            }

            if (timeUntilDeparture.TotalMinutes < -30) // 30 minutes after departure
            {
                return (false, "Boarding time has expired");
            }

            return (true, "Booking is valid for boarding");
        }
    }

    public class ScanBookingRequest
    {
        public string QRCodeData { get; set; } = string.Empty;
    }

    public class ValidateBookingRequest
    {
        public string BookingCode { get; set; } = string.Empty;
    }

    public class ScanImageRequest
    {
        [Required(ErrorMessage = "QR code image is required.")]
        public IFormFile QRImage { get; set; } = null!;
    }

    public class BookingScanResponse
    {
        public string BookingCode { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = string.Empty;
        public string? PaymentStatus { get; set; }
        public string? ContactName { get; set; } = string.Empty;
        public string? ContactPhone { get; set; } = string.Empty;
        public int TripId { get; set; }
        public string? TripCode { get; set; }
        public string? TrainNumber { get; set; }
        public string? DepartureStation { get; set; }
        public string? ArrivalStation { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public decimal TotalPrice { get; set; }
        public int TicketCount { get; set; }
    }

    public class BookingDetailsResponse
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = string.Empty;
        public string? PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public bool IsGuestBooking { get; set; }
        public string ContactInfo { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? TripCode { get; set; }
        public string? TrainNumber { get; set; }
        public string? DepartureStation { get; set; }
        public string? ArrivalStation { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public decimal TotalPrice { get; set; }
        public List<TicketInfo> Tickets { get; set; } = new List<TicketInfo>();

        
    }

    public class TicketInfo
    {
        public string TicketCode { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string? PassengerPhone { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string SeatInfo { get; set; } = string.Empty;
    }
}