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
        Task<BookingDetailsResponse?> GetBookingDetailsAsync(int bookingId);
        Task<Booking?> GetBookingByIdAsync(int bookingId);
        Task<BookingDetailsResponse?> GetBookingByCodeAsync(string bookingCode);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<bool> ExtendBookingAsync(int bookingId);
        Task<List<BookingDetailsResponse>> GetUserBookingsAsync(int userId, string? status = null, int page = 1, int pageSize = 10);

        // Guest booking methods
        Task<BookingDetailsResponse?> LookupGuestBookingAsync(GuestBookingLookupRequest request);
        Task<List<BookingDetailsResponse>> GetGuestBookingsAsync(string phone, string email);
        Task<UserBookingStatsResponse> GetUserBookingStatsAsync(int userId);
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
                if (request.Tickets == null || !request.Tickets.Any())
                    return Fail("Danh sách vé không được để trống");

                foreach (var t in request.Tickets)
                {
                    if (string.IsNullOrWhiteSpace(t.PassengerName))
                        return Fail("Tên hành khách không được để trống");

                    if (string.IsNullOrWhiteSpace(t.PassengerPhone))
                        return Fail("Số điện thoại hành khách không được để trống");

                    if (string.IsNullOrWhiteSpace(t.PassengerEmail))
                        return Fail("Email hành khách không được để trống");
                }

                var firstTicket = request.Tickets.First();
                if (request.IsGuestBooking)
                {
                    request.ContactName ??= firstTicket.PassengerName;
                    request.ContactPhone ??= firstTicket.PassengerPhone;
                    request.ContactEmail ??= firstTicket.PassengerEmail;
                }

                var trip = await _context.Trip.Include(t => t.Route)
                                              .FirstOrDefaultAsync(t => t.TripId == request.TripId);
                if (trip == null)
                    return Fail("Không tìm thấy chuyến đi");

                var segmentIds = await _routeService.GetSegmentIdsByRouteAsync(
                    trip.RouteId, request.DepartureStationId, request.ArrivalStationId);

                if (!segmentIds.Any())
                    return Fail("Không hợp lệ: các chặng không tồn tại trong tuyến");

                var bookingCode = GenerateBookingCode(request.IsGuestBooking);

                var booking = new Booking
                {
                    TripId = trip.TripId,
                    BookingStatus = "Temporary",
                    ExpirationTime = DateTime.UtcNow.AddMinutes(5),
                    UserId = request.UserId,
                    BookingCode = bookingCode,
                    ContactName = request.ContactName ?? string.Empty,
                    ContactPhone = request.ContactPhone ?? string.Empty,
                    ContactEmail = request.ContactEmail ?? string.Empty,
                    Tickets = new List<Ticket>()
                };

                decimal totalBookingPrice = 0;

                foreach (var ticketReq in request.Tickets)
                {
                    var isAvailable = await _seatService.IsSeatAvailableForSegmentsAsync(
                        request.TripId, ticketReq.SeatId, segmentIds);

                    if (!isAvailable)
                        return Fail($"Ghế {ticketReq.SeatId} đã được đặt");

                    var seat = await _context.Seat.FirstOrDefaultAsync(s => s.SeatId == ticketReq.SeatId);
                    if (seat == null)
                        throw new Exception($"❌ SeatId {ticketReq.SeatId} không tồn tại trong DB");

                    var ticket = new Ticket
                    {
                        TicketCode = $"{bookingCode}-{ticketReq.SeatId}",
                        TripId = request.TripId,
                        PassengerName = ticketReq.PassengerName,
                        PassengerPhone = ticketReq.PassengerPhone,
                        PassengerIdCard = ticketReq.PassengerIdCard,
                        TotalPrice = 0
                    };

                    foreach (var segmentId in segmentIds)
                    {
                        _context.SeatSegment.Add(new SeatSegment
                        {
                            TripId = request.TripId,
                            SeatId = ticketReq.SeatId,
                            SegmentId = segmentId,
                            Status = "TemporaryReserved",
                            ReservedAt = DateTime.UtcNow,
                            Booking = booking
                        });

                        var segmentPrice = await _pricingService.CalculateSegmentPriceAsync(
                            request.TripId, ticketReq.SeatId, segmentId);

                        ticket.TotalPrice += segmentPrice;
                    }

                    totalBookingPrice += ticket.TotalPrice;
                    booking.Tickets.Add(ticket);
                }

                booking.TotalPrice = totalBookingPrice;
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreateBookingResponse
                {
                    Success = true,
                    BookingId = booking.BookingId,
                    BookingCode = booking.BookingCode,
                    TotalPrice = totalBookingPrice,
                    ExpirationTime = booking.ExpirationTime,
                    IsGuestBooking = request.IsGuestBooking,
                    LookupPhone = request.IsGuestBooking ? request.ContactPhone : null,
                    LookupEmail = request.IsGuestBooking ? request.ContactEmail : null,
                    Message = request.IsGuestBooking
                        ? "Đặt chỗ thành công! Vui lòng lưu mã booking để tra cứu."
                        : "Đặt chỗ thành công! Vui lòng hoàn tất thanh toán trong 5 phút."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Booking lỗi: {Message} | StackTrace: {StackTrace}", ex.Message, ex.StackTrace);

                return Fail("Lỗi hệ thống khi tạo booking");
            }

            CreateBookingResponse Fail(string msg) => new CreateBookingResponse { Success = false, Message = msg };
        }



        /// <summary>
        /// 🔍 Tra cứu booking guest
        /// </summary>
        public async Task<BookingDetailsResponse?> LookupGuestBookingAsync(GuestBookingLookupRequest request)
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
                return MapToBookingDetailsResponse(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up guest booking with code {BookingCode}", request.BookingCode);
                return null;
            }
        }
        #region Existing Methods (Updated)

        public async Task<BookingDetailsResponse?> GetBookingByCodeAsync(string bookingCode)
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

                return booking != null ? MapToBookingDetailsResponse(booking) : null;
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

        private static BookingDetailsResponse MapToBookingDetailsResponse(Booking booking)
        {
            var seat = booking.SeatSegments.FirstOrDefault()?.Seat;
            var ticket = booking.Tickets.FirstOrDefault();

            return new BookingDetailsResponse
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                TripCode = booking.Trip.TripCode,
                TrainNumber = booking.Trip.Train.TrainNumber,
                PassengerName = booking.PassengerName,
                PassengerPhone = booking.PassengerPhone,
                PassengerEmail = booking.PassengerEmail,
                DepartureStation = booking.Trip.Route.DepartureStation.StationName,
                ArrivalStation = booking.Trip.Route.ArrivalStation.StationName,
                DepartureTime = booking.Trip.DepartureTime,
                ArrivalTime = booking.Trip.ArrivalTime,
                SeatNumber = seat?.SeatNumber ?? "",
                CarriageNumber = seat?.Carriage.CarriageNumber ?? "",
                TotalPrice = booking.TotalPrice,
                BookingStatus = booking.BookingStatus,
                TicketCode = ticket?.TicketCode,
                CreatedAt = booking.CreatedAt,
                ConfirmedAt = booking.ConfirmedAt,
                ExpirationTime = booking.ExpirationTime,
                IsGuestBooking = booking.IsGuestBooking,
                ContactInfo = booking.ContactInfo
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
                booking.PaymentStatus = "Paid";

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
                        PassengerName = booking.PassengerName,
                        PassengerIdCard = booking.PassengerIdCard,
                        PassengerPhone = booking.PassengerPhone,
                        TotalPrice = booking.TotalPrice,
                        FinalPrice = booking.TotalPrice,
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

        public async Task<BookingDetailsResponse?> GetBookingDetailsAsync(int bookingId)
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

                return booking != null ? MapToBookingDetailsResponse(booking) : null;
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

        public async Task<List<BookingDetailsResponse>> GetUserBookingsAsync(int userId, string? status = null, int page = 1, int pageSize = 10)
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

                return bookings.Select(MapToBookingDetailsResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings for user {UserId}", userId);
                return new List<BookingDetailsResponse>();
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
                    TotalSpent = bookings.Where(b => b.BookingStatus == "Confirmed").Sum(b => b.TotalPrice),
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

        public Task<List<BookingDetailsResponse>> GetGuestBookingsAsync(string phone, string email)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}