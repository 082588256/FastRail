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
        public async Task<ActionResult<ApiResponse<TicketDetailsResponse>>> ScanTicket(
            [FromBody] ScanTicketRequest request,
            [FromHeader(Name = "Authorization")] string? authorization)
        {
            try
            {
                // Validate STAFF token
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return Unauthorized(new ApiResponse<TicketDetailsResponse>
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

                    return Unauthorized(new ApiResponse<TicketDetailsResponse>
                    {
                        Success = false,
                        Message = "Invalid or expired token",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                if (string.IsNullOrWhiteSpace(request.QRCodeData))
                {
                    return BadRequest(new ApiResponse<TicketDetailsResponse>

                    {
                        Success = false,
                        Message = "QR code data is required",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Decode QR code to get ticket code
                var ticketCode = await _qrService.DecodeQRCodeAsync(request.QRCodeData);
                if (string.IsNullOrWhiteSpace(ticketCode))
                {
                    return BadRequest(new ApiResponse<TicketDetailsResponse>

                    {
                        Success = false,
                        Message = "Invalid QR code format",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Get booking details by ticket code

                var ticket = await _context.Ticket
                    .Include(t => t.Booking)
                    .Include(t => t.Trip)
                    .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
                if (ticket == null)
                {
                    return NotFound(new ApiResponse<TicketDetailsResponse>

                    {
                        Success = false,
                        Message = "Ticket not found",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Validate ticket for boarding

                var validationResult = await ValidateTicketForBoarding(ticket.Booking.BookingStatus, ticket.Trip.DepartureTime);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<TicketDetailsResponse>

                    {
                        Success = false,
                        Message = validationResult.Message,
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                _logger.LogInformation("Ticket {TicketCode} scanned successfully by staff", ticketCode);


                // Map Ticket to TicketDetailsResponse before returning
                var ticketDetails = new TicketDetailsResponse
                {
                    BookingId = ticket.BookingId,
                    BookingCode = ticket.Booking.BookingCode,
                    BookingStatus = ticket.Booking.BookingStatus,
                    CreatedAt = ticket.Booking.CreatedAt,
                    ConfirmedAt = ticket.Booking.ConfirmedAt,
                    ExpirationTime = ticket.Booking.ExpirationTime,
                    IsGuestBooking = ticket.Booking.IsGuestBooking,
                    ContactInfo = ticket.Booking.ContactInfo,
                    ContactName = ticket.Booking.ContactName,
                    ContactPhone = ticket.Booking.ContactPhone,
                    ContactEmail = ticket.Booking.ContactEmail,
                    TicketCode = ticket.TicketCode,
                    PassengerName = ticket.PassengerName,
                    PassengerPhone = ticket.PassengerPhone,
                    PassengerIdCard = ticket.PassengerIdCard,
                    TotalPrice = ticket.TotalPrice,
                    Status = ticket.Status,
                    TripCode = ticket.Trip?.TripCode,
                    TrainNumber = ticket.Trip?.Train?.TrainNumber,
                    DepartureStation = ticket.Trip?.Route?.DepartureStation?.StationName,
                    ArrivalStation = ticket.Trip?.Route?.ArrivalStation?.StationName,
                    DepartureTime = ticket.Trip?.DepartureTime,
                    ArrivalTime = ticket.Trip?.ArrivalTime,
                    SeatNumber = null, // Set if you have seat info
                    CarriageNumber = null // Set if you have carriage info
                };

                return Ok(new ApiResponse<TicketDetailsResponse>
                {
                    Success = true,
                    Data = ticketDetails,
                    Message = "Ticket scanned successfully",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning ticket");
                return StatusCode(500, new ApiResponse<TicketDetailsResponse>
                {
                    Success = false,
                    Message = "An error occurred while scanning ticket",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponse<object>>> ValidateTicket(
            [FromBody] ValidateTicketRequest request,
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

                if (string.IsNullOrWhiteSpace(request.TicketCode))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Ticket code is required",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Get booking details by ticket code
                var ticket = await _context.Ticket
                    .Include(t => t.Booking)
                    .Include(t => t.Trip)
                    .FirstOrDefaultAsync(t => t.TicketCode == request.TicketCode);
                if (ticket == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Ticket not found",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Mark ticket as checked in
                var checkInResult = await _bookingService.ConfirmBookingAsync(ticket.BookingId, "QR_SCAN");

                if (!checkInResult)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to check in ticket",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                _logger.LogInformation("Ticket {TicketCode} validated and checked in by staff", request.TicketCode);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Ticket validated and checked in successfully",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ticket {TicketCode}", request.TicketCode);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while validating ticket",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet("generate/{ticketCode}")]
        public async Task<ActionResult> GenerateQRCode(string ticketCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ticketCode))
                {
                    return BadRequest("Ticket code is required");
                }

                var qrCodeImage = await _qrService.GenerateQRCodeImageAsync(ticketCode);
                return File(qrCodeImage, "image/png", $"ticket_{ticketCode}.png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for ticket {TicketCode}", ticketCode);
                return StatusCode(500, "An error occurred while generating QR code");
            }
        }

        [HttpPost("scan-image")]

        public async Task<ActionResult<ApiResponse<TicketScanResponse>>> ScanTicketFromImage(
            [FromForm] ScanImageRequest request,
            [FromHeader(Name = "Authorization")] string? authorization)
        {
            // 1. Validate Authorization
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return Unauthorized(new ApiResponse<TicketScanResponse>
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
                return Unauthorized(new ApiResponse<TicketScanResponse>
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
                return BadRequest(new ApiResponse<TicketScanResponse>
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
                return BadRequest(new ApiResponse<TicketScanResponse>
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
                return BadRequest(new ApiResponse<TicketScanResponse>
                {
                    Success = false,
                    Message = "Uploaded image is empty.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // 5. Decode QR code from image
            string ticketCode;
            try
            {
                ticketCode = await _qrService.DecodeQRCodeFromImageAsync(imageData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decoding QR code from image");
                return BadRequest(new ApiResponse<TicketScanResponse>
                {
                    Success = false,
                    Message = "Failed to decode QR code from image. Ensure the image is clear and contains a valid QR code.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(new ApiResponse<TicketScanResponse>
                {
                    Success = false,
                    Message = "No valid QR code found in the image.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // 6. Lookup ticket by TicketCode
            var ticket = await _context.Ticket
                .Include(t => t.Booking)
                .Include(t => t.Trip)
                .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
            if (ticket == null)
            {
                return NotFound(new ApiResponse<TicketScanResponse>
                {
                    Success = false,
                    Message = "Ticket not found.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            // 7. Validate ticket status for boarding
            if (ticket.Status != "Valid")
            {
                return BadRequest(new ApiResponse<TicketScanResponse>
                {
                    Success = false,
                    Message = $"Ticket is not valid for boarding. Status: {ticket.Status}",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            _logger.LogInformation("Ticket {TicketCode} scanned from image successfully by staff", ticketCode);

            // 8. Return ticket info
            var response = new TicketScanResponse
            {
                TicketCode = ticket.TicketCode,
                PassengerName = ticket.PassengerName,
                PassengerPhone = ticket.PassengerPhone,
                BookingCode = ticket.Booking.BookingCode,
                TripId = ticket.TripId,
                Status = ticket.Status,
                DepartureTime = ticket.Trip.DepartureTime // Set from Trip entity
                // Add more fields as needed
            };

            return Ok(new ApiResponse<TicketScanResponse>
            {
                Success = true,
                Data = response,
                Message = "Ticket scanned successfully from image.",
                RequestId = HttpContext.TraceIdentifier
            });
        }

        // Remove or refactor this method to not use BookingDetailsResponse.DepartureTime
        // Instead, use ticket.Trip.DepartureTime or pass DepartureTime as a parameter
        private async Task<(bool IsValid, string Message)> ValidateTicketForBoarding(string ticketStatus, DateTime departureTime)
        {
            // Check if ticket is valid for boarding
            if (ticketStatus != "Valid")
            {
                return (false, $"Ticket is not valid for boarding. Status: {ticketStatus}");
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

            return (true, "Ticket is valid for boarding");
        }
    }

    public class ScanTicketRequest
    {
        public string QRCodeData { get; set; } = string.Empty;
    }

    public class ValidateTicketRequest
    {
        public string TicketCode { get; set; } = string.Empty;
    }

    public class ScanImageRequest
    {
        [Required(ErrorMessage = "QR code image is required.")]
        public IFormFile QRImage { get; set; } = null!;
    }

    public class TicketScanResponse
    {
        public string TicketCode { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerPhone { get; set; } = string.Empty;
        public string BookingCode { get; set; } = string.Empty;
        public int TripId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; } // Add this if needed by frontend
        // Add more fields as needed
    }
}