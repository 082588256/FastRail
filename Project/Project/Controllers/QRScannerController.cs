using Microsoft.AspNetCore.Mvc;
using Project.DTO;
using Project.DTOs;
using Project.Services;

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

        public QRScannerController(
            IQRService qrService,
            IBookingService bookingService,
            IAuthService authService,
            ILogger<QRScannerController> logger)
        {
            _qrService = qrService;
            _bookingService = bookingService;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("scan")]
        public async Task<ActionResult<ApiResponse<BookingDetailsResponse>>> ScanTicket(
            [FromBody] ScanTicketRequest request,
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

                // Decode QR code to get ticket code
                var ticketCode = await _qrService.DecodeQRCodeAsync(request.QRCodeData);
                if (string.IsNullOrWhiteSpace(ticketCode))
                {
                    return BadRequest(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "Invalid QR code format",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Get booking details by ticket code
                var booking = await _bookingService.GetBookingByCodeAsync(ticketCode);
                if (booking == null)
                {
                    return NotFound(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "Ticket not found",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Validate ticket for boarding
                var validationResult = await ValidateTicketForBoarding(booking);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = validationResult.Message,
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                _logger.LogInformation("Ticket {TicketCode} scanned successfully by staff", ticketCode);

                return Ok(new ApiResponse<BookingDetailsResponse>
                {
                    Success = true,
                    Data = booking,
                    Message = "Ticket scanned successfully",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning ticket");
                return StatusCode(500, new ApiResponse<BookingDetailsResponse>
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
                var booking = await _bookingService.GetBookingByCodeAsync(request.TicketCode);
                if (booking == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Ticket not found",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Mark ticket as checked in
                var checkInResult = await _bookingService.ConfirmBookingAsync(booking.BookingId, "QR_SCAN");
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
        public async Task<ActionResult<ApiResponse<BookingDetailsResponse>>> ScanTicketFromImage(
            [FromForm] ScanImageRequest request,
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

                if (request.QRImage == null || request.QRImage.Length == 0)
                {
                    return BadRequest(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "QR code image is required",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
                var fileExtension = Path.GetExtension(request.QRImage.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "Invalid file type. Only JPG, PNG, and BMP are allowed.",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Read image data
                using var ms = new MemoryStream();
                await request.QRImage.CopyToAsync(ms);
                var imageData = ms.ToArray();

                // Decode QR code from image
                var ticketCode = await _qrService.DecodeQRCodeFromImageAsync(imageData);
                if (string.IsNullOrWhiteSpace(ticketCode))
                {
                    return BadRequest(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "No valid QR code found in the image",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Get booking details by ticket code
                var booking = await _bookingService.GetBookingByCodeAsync(ticketCode);
                if (booking == null)
                {
                    return NotFound(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = "Ticket not found",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                // Validate ticket for boarding
                var validationResult = await ValidateTicketForBoarding(booking);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<BookingDetailsResponse>
                    {
                        Success = false,
                        Message = validationResult.Message,
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                _logger.LogInformation("Ticket {TicketCode} scanned from image successfully by staff", ticketCode);

                return Ok(new ApiResponse<BookingDetailsResponse>
                {
                    Success = true,
                    Data = booking,
                    Message = "Ticket scanned successfully from image",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning ticket from image");
                return StatusCode(500, new ApiResponse<BookingDetailsResponse>
                {
                    Success = false,
                    Message = "An error occurred while scanning ticket from image",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        private async Task<(bool IsValid, string Message)> ValidateTicketForBoarding(BookingDetailsResponse booking)
        {
            // Check if ticket is valid for boarding
            if (booking.BookingStatus != "Confirmed")
            {
                return (false, $"Ticket is not valid for boarding. Status: {booking.BookingStatus}");
            }

            // Check if trip time is valid (within 2 hours before departure)
            var timeUntilDeparture = booking.DepartureTime - DateTime.UtcNow;
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
        public IFormFile? QRImage { get; set; }
    }
} 