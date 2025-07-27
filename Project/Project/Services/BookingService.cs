using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.DTOs;
using Project.Services.Route;

namespace Project.Services
{
    public interface IBookingService
    {
        Task<CreateBookingResponse> CreateTemporaryBookingAsync(CreateBookingRequest request);
        Task<bool> ConfirmBookingAsync(int bookingId, string? transactionId = null);
        Task<TicketDetailsResponse?> GetBookingDetailsAsync(int bookingId);
        Task<Booking?> GetBookingByIdAsync(int bookingId);
        Task<TicketDetailsResponse?> GetBookingByCodeAsync(string bookingCode);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<bool> ExtendBookingAsync(int bookingId);
        Task<List<TicketDetailsResponse>> GetUserBookingsAsync(int userId, string? status = null, int page = 1, int pageSize = 10);

        // Guest booking methods
        Task<TicketDetailsResponse?> LookupGuestBookingAsync(GuestBookingLookupRequest request);
        Task<List<TicketDetailsResponse>> GetGuestBookingsAsync(string phone, string email);
        Task<UserBookingStatsResponse> GetUserBookingStatsAsync(int userId);

        Task<int> CountTicketsTodayAsync();
        Task<long> CalculateTodayRevenueAsync();
    }

    public class BookingService : IBookingService
    {
        private readonly FastRailDbContext _context;
        private readonly IPricingService _pricingService;
        private readonly ILogger<BookingService> _logger;
        private readonly IRouteService _routeService;   
        private readonly ISeatService _seatService;

        public BookingService(FastRailDbContext context, IPricingService pricingService, ILogger<BookingService> logger, IRouteService routeService, ISeatService seatService)
        {
            _context = context;
            _pricingService = pricingService;
            _logger = logger;
            _routeService = routeService;
            _seatService = seatService;
        }

        /// <summary>
        /// 🎫 Tạo booking tạm thời (hỗ trợ cả user và guest)
        /// </summary>
        public async Task<CreateBookingResponse> CreateTemporaryBookingAsync(CreateBookingRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate guest contact info
                if (request.IsGuestBooking)
                {
                    if (string.IsNullOrWhiteSpace(request.ContactName))
                        return new CreateBookingResponse { Success = false, Message = "Contact name is required for guest booking" };
                    if (string.IsNullOrWhiteSpace(request.ContactPhone))
                        return new CreateBookingResponse { Success = false, Message = "Contact phone is required for guest booking" };
                    if (string.IsNullOrWhiteSpace(request.ContactEmail))
                        return new CreateBookingResponse { Success = false, Message = "Contact email is required for guest booking" };
                }

                // Validate passenger info (for ticket)
                // (Assume you add these fields to the request DTO for ticket creation)
                if (string.IsNullOrWhiteSpace(request.PassengerName))
                    return new CreateBookingResponse { Success = false, Message = "Passenger name is required" };
                if (string.IsNullOrWhiteSpace(request.PassengerPhone))
                    return new CreateBookingResponse { Success = false, Message = "Passenger phone is required" };
                if (string.IsNullOrWhiteSpace(request.PassengerEmail))
                    return new CreateBookingResponse { Success = false, Message = "Passenger email is required" };

                // Check seat availability
                var seatAvailable = await IsSeatAvailableAsync(request.TripId, request.SeatId);
                if (!seatAvailable)
                {
                    return new CreateBookingResponse
                    {
                        Success = false,
                        Message = "Seat is already booked or does not exist"
                    };
                }

                // Calculate price
                var totalPrice = await _pricingService.CalculateTotalPriceAsync(request.SeatId, new List<int> { 1 });

                // Generate booking code
                var bookingCode = GenerateBookingCode(request.IsGuestBooking);

                // Create booking (minimal info)
                var booking = new Booking
                {
                    UserId = request.UserId, // null for guest
                    TripId = request.TripId,
                    BookingCode = bookingCode,
                    BookingStatus = "Confirmed",
                    PaymentStatus = "Completed",
                    ExpirationTime = null,
                    ContactName = request.ContactName?.Trim(),
                    ContactPhone = request.ContactPhone?.Trim(),
                    ContactEmail = request.ContactEmail?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    ConfirmedAt = DateTime.UtcNow
                };
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Mark seat as booked
                var seatSegment = new SeatSegment
                {
                    TripId = request.TripId,
                    SeatId = request.SeatId,
                    SegmentId = 1, // Simplified for now
                    BookingId = booking.BookingId,
                    Status = "Booked",
                    ReservedAt = DateTime.UtcNow,
                    BookedAt = DateTime.UtcNow
                };
                _context.SeatSegment.Add(seatSegment);
                await _context.SaveChangesAsync();

                // Create ticket
                var ticketCode = GenerateTicketCode();
                var ticket = new Ticket
                {
                    BookingId = booking.BookingId,
                    UserId = booking.UserId, // null for guest
                    TripId = booking.TripId,
                    TicketCode = ticketCode,
                    PassengerName = request.PassengerName.Trim(),
                    PassengerIdCard = request.PassengerIdCard?.Trim(),
                    PassengerPhone = request.PassengerPhone.Trim(),
                    TotalPrice = totalPrice,
                    FinalPrice = totalPrice,
                    Status = "Valid",
                    PurchaseTime = DateTime.UtcNow
                };
                _context.Ticket.Add(ticket);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Created guest booking {BookingId} and ticket {TicketCode}", booking.BookingId, ticketCode);

                return new CreateBookingResponse
                {
                    Success = true,
                    BookingId = booking.BookingId,
                    BookingCode = booking.BookingCode,
                    TotalPrice = totalPrice,
                    ExpirationTime = null,
                    IsGuestBooking = request.IsGuestBooking,
                    LookupPhone = request.IsGuestBooking ? request.ContactPhone : null,
                    LookupEmail = request.IsGuestBooking ? request.ContactEmail : null,
                    Message = "Booking and ticket created successfully!",
                    TicketCode = ticketCode // Add TicketCode to response
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating guest booking and ticket for trip {TripId}, seat {SeatId}", request.TripId, request.SeatId);
                return new CreateBookingResponse
                {
                    Success = false,
                    Message = "System error while creating booking and ticket"
                };
            }

            CreateBookingResponse Fail(string msg) => new CreateBookingResponse { Success = false, Message = msg };
        }



        /// <summary>
        /// 🔍 Tra cứu booking guest
        /// </summary>
        public async Task<TicketDetailsResponse?> LookupGuestBookingAsync(GuestBookingLookupRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.BookingCode))
                {
                    return null;
                }

                var booking = await _context.Bookings
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Train)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DepartureStation)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.ArrivalStation)
                    .Include(b => b.SeatSegments)
                        .ThenInclude(ss => ss.Seat)
                            .ThenInclude(s => s.Carriage)
                    .Include(b => b.Tickets)
                    .Where(b => b.BookingCode == request.BookingCode)
                    .FirstOrDefaultAsync();
                if (booking == null)
                {
                    return null;
                }

                //// Validate thông tin tra cứu cho guest booking
                //if (booking.IsGuestBooking)
                //{
                //    bool phoneMatch = string.IsNullOrWhiteSpace(request.Phone) ||
                //                    booking.ContactPhone == request.Phone ||
                //                    booking.PassengerPhone == request.Phone;

                //    bool emailMatch = string.IsNullOrWhiteSpace(request.Email) ||
                //                    booking.ContactEmail?.ToLower() == request.Email?.ToLower() ||
                //                    booking.PassengerEmail?.ToLower() == request.Email?.ToLower();

                //    if (!phoneMatch && !emailMatch)
                //    {
                //        return null; // Không match thông tin tra cứu
                //    }
                //}

                return MapToTicketDetailsResponse(booking);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up guest booking with code {BookingCode}", request.BookingCode);
                return null;
            }
        }

        /// <summary>
        /// 📋 Lấy booking của guest theo phone/email
        /// </summary>
        public async Task<List<TicketDetailsResponse>> GetGuestBookingsAsync(string phone, string email)
        {
            try
            {
                var query = _context.Bookings
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Train)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DepartureStation)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.ArrivalStation)
                    .Include(b => b.SeatSegments)
                        .ThenInclude(ss => ss.Seat)
                            .ThenInclude(s => s.Carriage)
                    .Include(b => b.Tickets)
                    .Where(b => b.UserId == null); // Chỉ guest bookings

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    query = query.Where(b => b.ContactPhone == phone);
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    query = query.Where(b => b.ContactEmail.ToLower() == email.ToLower());
                }

                var bookings = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                return bookings.Select(MapToTicketDetailsResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest bookings for phone {Phone}, email {Email}", phone, email);
                return new List<TicketDetailsResponse>();
            }
        }
        
        #region Existing Methods (Updated)

        public async Task<TicketDetailsResponse?> GetBookingByCodeAsync(string bookingCode)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Train)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DepartureStation)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.ArrivalStation)
                    .Include(b => b.SeatSegments)
                        .ThenInclude(ss => ss.Seat)
                            .ThenInclude(s => s.Carriage)
                    .Include(b => b.Tickets)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.BookingCode.ToUpper() == bookingCode.ToUpper());

                return booking != null ? MapToTicketDetailsResponse(booking) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking by code {BookingCode}", bookingCode);
                return null;
            }
        }

        // ... Các method khác giữ nguyên nhưng cập nhật MapToBookingDetailsResponse

        #endregion

        #region Private Helper Methods

        public async Task<bool> IsSeatAvailableForSegmentsAsync(int tripId, int seatId, List<int> segmentIds)
        {
            return !await _context.SeatSegment
                .AnyAsync(ss =>
                    ss.TripId == tripId &&
                    ss.SeatId == seatId &&
                    segmentIds.Contains(ss.SegmentId) &&
                    (ss.Status == "TemporaryReserved" || ss.Status == "Booked"));
        }

        private static string GenerateBookingCode(bool isGuestBooking)
        {
            var prefix = isGuestBooking ? "GB" : "BK"; // GB = Guest Booking, BK = Regular Booking
            return $"{prefix}{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        private static string GenerateTicketCode()
        {
            return $"TK{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }


        // Replace MapToBookingDetailsResponse with MapToTicketDetailsResponse
        private static TicketDetailsResponse MapToTicketDetailsResponse(Booking booking)

        {
            var seat = booking.SeatSegments.FirstOrDefault()?.Seat;
            var ticket = booking.Tickets.FirstOrDefault();
            var trip = booking.Trip;
            return new TicketDetailsResponse
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                BookingStatus = booking.BookingStatus,
                CreatedAt = booking.CreatedAt,
                ConfirmedAt = booking.ConfirmedAt,
                ExpirationTime = booking.ExpirationTime,
                IsGuestBooking = booking.IsGuestBooking,
                ContactInfo = booking.ContactInfo,
                ContactName = booking.ContactName,
                ContactPhone = booking.ContactPhone,
                ContactEmail = booking.ContactEmail,
                TicketCode = ticket?.TicketCode,
                PassengerName = ticket?.PassengerName,
                PassengerPhone = ticket?.PassengerPhone,
                PassengerIdCard = ticket?.PassengerIdCard,
                TotalPrice = ticket?.TotalPrice ?? 0,
                Status = ticket?.Status,
                TripCode = trip?.TripCode,
                TrainNumber = trip?.Train?.TrainNumber,
                DepartureStation = trip?.Route?.DepartureStation?.StationName,
                ArrivalStation = trip?.Route?.ArrivalStation?.StationName,
                DepartureTime = trip?.DepartureTime,
                ArrivalTime = trip?.ArrivalTime,
                SeatNumber = seat?.SeatNumber,
                CarriageNumber = seat?.Carriage?.CarriageNumber
            };
        }

        public async Task<bool> ConfirmBookingAsync(int bookingId, string? transactionId = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.SeatSegments)
                    .Include(b => b.Tickets)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return false;
                }

                // Update booking status
                booking.BookingStatus = "Confirmed";
                booking.ConfirmedAt = DateTime.UtcNow;
                booking.PaymentStatus = "Completed";
              // Update seat segments
                foreach (var seatSegment in booking.SeatSegments)
                {
                    seatSegment.Status = "Booked";
                }

                // Create or update ticket
                var ticket = booking.Tickets.FirstOrDefault();
                if (ticket == null)
                {
                    ticket = new Ticket
                    {
                        BookingId = booking.BookingId,
                        UserId = booking.UserId ?? 0, // For guest bookings, use 0
                        TripId = booking.TripId,
                        TicketCode = GenerateTicketCode(),
                        PassengerName = booking.ContactName ?? "Guest",
                        PassengerIdCard = null,
                        PassengerPhone = booking.ContactPhone ?? "",
                        TotalPrice = 0, // Set to 0 or calculate as needed
                        FinalPrice = 0, // Set to 0 or calculate as needed

                        Status = "Valid",
                        PurchaseTime = DateTime.UtcNow
                    };

                    _context.Ticket.Add(ticket);
                }
                else
                {
                    ticket.Status = "Valid";
                    ticket.CheckInTime = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Booking {BookingId} confirmed successfully", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
                return false;
            }
        }


        public async Task<TicketDetailsResponse?> GetBookingDetailsAsync(int bookingId)

        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Train)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DepartureStation)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.ArrivalStation)
                    .Include(b => b.SeatSegments)
                        .ThenInclude(ss => ss.Seat)
                            .ThenInclude(s => s.Carriage)
                    .Include(b => b.Tickets)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);
                return booking != null ? MapToTicketDetailsResponse(booking) : null;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking details for ID {BookingId}", bookingId);
                return null;
            }
        }

        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            try
            {
                return await _context.Bookings
                    .Include(b => b.Trip)
                    .Include(b => b.SeatSegments)
                    .Include(b => b.Tickets)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking by ID {BookingId}", bookingId);
                return null;
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.SeatSegments)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                    return false;

                booking.BookingStatus = "Cancelled";
                booking.CancelledAt = DateTime.UtcNow;

                // Release seat segments
                foreach (var seatSegment in booking.SeatSegments)
                {
                    seatSegment.Status = "Available";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Booking {BookingId} cancelled successfully", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
                return false;
            }
        }

        public async Task<bool> ExtendBookingAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                    return false;

                booking.ExpirationTime = DateTime.UtcNow.AddMinutes(5);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Booking {BookingId} extended successfully", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending booking {BookingId}", bookingId);
                return false;
            }
        }

        public async Task<List<TicketDetailsResponse>> GetUserBookingsAsync(int userId, string? status = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Bookings
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Train)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DepartureStation)
                    .Include(b => b.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.ArrivalStation)
                    .Include(b => b.SeatSegments)
                        .ThenInclude(ss => ss.Seat)
                            .ThenInclude(s => s.Carriage)
                    .Include(b => b.Tickets)
                    .Where(b => b.UserId == userId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(b => b.BookingStatus == status);
                }

                var bookings = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return bookings.Select(MapToTicketDetailsResponse).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings for user {UserId}", userId);
                return new List<TicketDetailsResponse>();
            }
        }

        public async Task<UserBookingStatsResponse> GetUserBookingStatsAsync(int userId)
        {
            try
            {
                var bookings = await _context.Bookings
                    .Where(b => b.UserId == userId)
                    .ToListAsync();

                var stats = new UserBookingStatsResponse
                {
                    TotalBookings = bookings.Count,
                    ConfirmedBookings = bookings.Count(b => b.BookingStatus == "Confirmed"),
                    CancelledBookings = bookings.Count(b => b.BookingStatus == "Cancelled"),
                    ExpiredBookings = bookings.Count(b => b.BookingStatus == "Expired"),
                    TotalSpent = bookings.Where(b => b.BookingStatus == "Confirmed").SelectMany(b => b.Tickets).Sum(t => t.TotalPrice),
                    LastBookingDate = bookings.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.CreatedAt,
                    NextTripDate = bookings.Where(b => b.BookingStatus == "Confirmed" && b.Trip.DepartureTime > DateTime.UtcNow)
                                         .OrderBy(b => b.Trip.DepartureTime)
                                         .FirstOrDefault()?.Trip.DepartureTime
                };

                if (stats.TotalBookings > 0)
                {
                    stats.AverageBookingValue = stats.TotalSpent / stats.ConfirmedBookings;
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user booking stats for user {UserId}", userId);
                return new UserBookingStatsResponse();
            }
        }
        private async Task<bool> IsSeatAvailableAsync(int tripId, int seatId)
        {
            return !await _context.SeatSegment.AnyAsync(ss =>
                ss.TripId == tripId &&
                ss.SeatId == seatId &&
                (ss.Status == "TemporaryReserved" || ss.Status == "Booked"));
        }

        public Task<int> CountTicketsTodayAsync()
        {
            throw new NotImplementedException();
        }

        public Task<long> CalculateTodayRevenueAsync()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}