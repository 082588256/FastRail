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

        public BookingService(FastRailDbContext context, IPricingService pricingService, ILogger<BookingService> logger)
        {
            _context = context;
            _pricingService = pricingService;
            _logger = logger;
        }

        /// <summary>
        /// 🎫 Tạo booking tạm thời (hỗ trợ cả user và guest)
        /// </summary>
        public async Task<CreateBookingResponse> CreateTemporaryBookingAsync(CreateBookingRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate thông tin hành khách (bắt buộc cho cả user và guest)
                if (string.IsNullOrWhiteSpace(request.PassengerName))
                {
                    return new CreateBookingResponse
                    {
                        Success = false,
                        Message = "Tên hành khách không được để trống"
                    };
                }

                if (string.IsNullOrWhiteSpace(request.PassengerPhone))
                {
                    return new CreateBookingResponse
                    {
                        Success = false,
                        Message = "Số điện thoại hành khách không được để trống"
                    };
                }

                if (string.IsNullOrWhiteSpace(request.PassengerEmail))
                {
                    return new CreateBookingResponse
                    {
                        Success = false,
                        Message = "Email hành khách không được để trống"
                    };
                }

                // Validate thông tin liên hệ cho guest booking
                if (request.IsGuestBooking)
                {
                    if (string.IsNullOrWhiteSpace(request.ContactName))
                        request.ContactName = request.PassengerName;

                    if (string.IsNullOrWhiteSpace(request.ContactPhone))
                        request.ContactPhone = request.PassengerPhone;

                    if (string.IsNullOrWhiteSpace(request.ContactEmail))
                        request.ContactEmail = request.PassengerEmail;
                }

                // Kiểm tra ghế có available không
                var seatAvailable = await IsSeatAvailableAsync(request.TripId, request.SeatId);
                if (!seatAvailable)
                {
                    return new CreateBookingResponse
                    {
                        Success = false,
                        Message = "Ghế đã có người đặt hoặc không tồn tại"
                    };
                }

                // Tính giá
                var totalPrice = await _pricingService.CalculateTotalPriceAsync(request.SeatId, new List<int> { 1 });

                // Tạo booking code
                var bookingCode = GenerateBookingCode(request.IsGuestBooking);

                // Tạo booking
                var booking = new Booking
                {
                    UserId = request.UserId, // Có thể null cho guest
                    TripId = request.TripId,
                    BookingCode = bookingCode,
                    BookingStatus = "Temporary",
                    TotalPrice = totalPrice,
                    ExpirationTime = DateTime.UtcNow.AddMinutes(5),

                    // Thông tin hành khách
                    PassengerName = request.PassengerName.Trim(),
                    PassengerPhone = request.PassengerPhone.Trim(),
                    PassengerEmail = request.PassengerEmail.Trim(),
                    PassengerIdCard = request.PassengerIdCard?.Trim(),
                    PassengerDateOfBirth = request.PassengerDateOfBirth,

                    // Thông tin liên hệ (cho guest)
                    ContactName = request.ContactName?.Trim(),
                    ContactPhone = request.ContactPhone?.Trim(),
                    ContactEmail = request.ContactEmail?.Trim(),

                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Đánh dấu ghế tạm giữ
                var seatSegment = new SeatSegment
                {
                    TripId = request.TripId,
                    SeatId = request.SeatId,
                    SegmentId = 1, // Đơn giản hóa cho PRN
                    BookingId = booking.BookingId,
                    Status = "TemporaryReserved",
                    ReservedAt = DateTime.UtcNow
                };

                _context.SeatSegment.Add(seatSegment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Created {BookingType} booking {BookingId} with code {BookingCode}",
                    request.IsGuestBooking ? "guest" : "user",
                    booking.BookingId,
                    bookingCode);

                return new CreateBookingResponse
                {
                    Success = true,
                    BookingId = booking.BookingId,
                    BookingCode = bookingCode,
                    TotalPrice = totalPrice,
                    ExpirationTime = booking.ExpirationTime,
                    IsGuestBooking = request.IsGuestBooking,
                    LookupPhone = request.IsGuestBooking ? request.ContactPhone : null,
                    LookupEmail = request.IsGuestBooking ? request.ContactEmail : null,
                    Message = request.IsGuestBooking ?
                        "Đặt chỗ thành công! Vui lòng lưu mã booking để tra cứu." :
                        "Đặt chỗ thành công! Vui lòng hoàn tất thanh toán trong 5 phút."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating temporary booking for trip {TripId}, seat {SeatId}",
                    request.TripId, request.SeatId);

                return new CreateBookingResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống khi tạo booking"
                };
            }
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

                return MapToBookingDetailsResponse(booking);
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
        public async Task<List<BookingDetailsResponse>> GetGuestBookingsAsync(string phone, string email)
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
                    query = query.Where(b => b.ContactPhone == phone || b.PassengerPhone == phone);
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    query = query.Where(b => b.ContactEmail.ToLower() == email.ToLower() ||
                                           b.PassengerEmail.ToLower() == email.ToLower());
                }

                var bookings = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                return bookings.Select(MapToBookingDetailsResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest bookings for phone {Phone}, email {Email}", phone, email);
                return new List<BookingDetailsResponse>();
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

        private async Task<bool> IsSeatAvailableAsync(int tripId, int seatId)
        {
            var isBooked = await _context.SeatSegment
                .AnyAsync(ss => ss.TripId == tripId &&
                               ss.SeatId == seatId &&
                               (ss.Status == "Booked" || ss.Status == "TemporaryReserved"));

            return !isBooked;
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

        #endregion
    }
}