using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.DTOs;

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
                    PassengerName = request.ContactName ?? string.Empty,
                    PassengerPhone = request.ContactPhone ?? string.Empty,
                    PassengerEmail = request.ContactEmail ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    Tickets = new List<Ticket>()
                };
                _context.Bookings.Add(booking);
                

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
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreateBookingResponse
                {
                    Success = true,
                    BookingId= booking.BookingId,
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

        public Task<bool> ConfirmBookingAsync(int bookingId, string? transactionId = null)
        {
            throw new NotImplementedException();
        }

        public Task<BookingDetailsResponse?> GetBookingDetailsAsync(int bookingId)
        {
            throw new NotImplementedException();
        }

        public Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CancelBookingAsync(int bookingId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExtendBookingAsync(int bookingId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BookingDetailsResponse>> GetUserBookingsAsync(int userId, string? status = null, int page = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<UserBookingStatsResponse> GetUserBookingStatsAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BookingDetailsResponse>> GetGuestBookingsAsync(string phone, string email)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    
}