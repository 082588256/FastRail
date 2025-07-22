using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Models;

namespace Project.Services
{
    public interface ITripService
    {
        Task<List<TripSearchResponse>> SearchTripsAsync(TripSearchRequest request);
        Task<List<SeatAvailabilityResponse>> GetAvailableSeatsAsync(int tripId, int fromStationId, int toStationId);
    }

    public class TripService : ITripService
    {
        private readonly FastRailDbContext _context;
        private readonly IPricingService _pricingService;

        public TripService(FastRailDbContext context, IPricingService pricingService)
        {
            _context = context;
            _pricingService = pricingService;
        }

        public async Task<List<TripSearchResponse>> SearchTripsAsync(TripSearchRequest request)
        {
            var searchDate = request.TravelDate.Date;
            var fromName = request.DepartureStationName;
            var toName = request.ArrivalStationName;

            var trips = await _context.Trip
                .Include(t => t.Train)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DepartureStation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.ArrivalStation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.RouteSegments)
                        .ThenInclude(rs => rs.FromStation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.RouteSegments)
                        .ThenInclude(rs => rs.ToStation)
                .Where(t => t.DepartureTime.Date == searchDate && t.IsActive)
                .Where(t => t.Route.RouteSegments.Any(rs =>
                    rs.FromStation.StationName == fromName &&
                    rs.ToStation.StationName == toName))
                .ToListAsync();

            var result = trips.Select(t => new TripSearchResponse
            {
                TripId = t.TripId,
                TripCode = t.TripCode,
                TrainNumber = t.Train.TrainNumber,
                TrainName = t.Train.TrainName,
                RouteName = t.Route.RouteName,
                DepartureStation = fromName,
                ArrivalStation = toName,
                DepartureTime = t.DepartureTime,
                ArrivalTime = t.ArrivalTime,
                EstimatedDurationMinutes = (int)(t.ArrivalTime - t.DepartureTime).TotalMinutes
            }).ToList();

            return result;
        }

        public async Task<List<SeatAvailabilityResponse>> GetAvailableSeatsAsync(int tripId, int fromStationId, int toStationId)
        {
            var expiredBookings = await _context.Bookings
    .Where(b => b.ExpirationTime < DateTime.Now)
    .ToListAsync();

            //foreach (var booking in expiredBookings)
            //{
            //    var relatedSeatSegments = _context.SeatSegment.Where(ss => ss.BookingId == booking.BookingId);
            //    _context.SeatSegment.RemoveRange(relatedSeatSegments);

            //    _context.Bookings.Remove(booking);
            //}
            // 1. Lấy trip và thông tin route
            var trip = await _context.Trip
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            var routeId = trip.RouteId;

            // 2. Lấy danh sách RouteSegment của toàn bộ tuyến
            var segments = await _context.RouteSegment
                .Where(rs => rs.RouteId == routeId)
                .OrderBy(rs => rs.Order)
                .ToListAsync();

            // 3. Tìm Order của từ và đến
            var fromOrder = segments.FirstOrDefault(s => s.FromStationId == fromStationId)?.Order ?? -1;
            var toOrder = segments.FirstOrDefault(s => s.FromStationId == toStationId)?.Order ?? segments.Max(s => s.Order) + 1;

            if (fromOrder == -1 || toOrder == -1 || fromOrder > toOrder)
                return new List<SeatAvailabilityResponse>(); // invalid segment range

            // 4. Lấy các RouteSegmentId người dùng sẽ đi qua
            var desiredSegmentIds = segments
                .Where(s => s.Order >= fromOrder && s.Order < toOrder)
                .Select(s => s.SegmentId)
                .ToList();

            // 5. Lấy tất cả ghế của đoàn tàu thuộc trip
            var allSeats = await _context.Seat
                .Include(s => s.Carriage)
                .Where(s => s.Carriage.Train.Trips.Any(t => t.TripId == tripId) && s.IsActive)
                .ToListAsync();

            // 6. Tìm các ghế đã bị đặt trong bất kỳ segment nào giao với desired segments
            var bookedSeatIds = await _context.SeatSegment
                .Where(ss => ss.TripId == tripId &&
                             desiredSegmentIds.Contains(ss.SegmentId) &&
                             (ss.Status == "Booked" || ss.Status == "TemporaryReserved" ))
                .Select(ss => ss.SeatId)
                .Distinct()
                .ToListAsync();

            // 7. Chuẩn bị danh sách trả về
            var result = allSeats.Select(seat =>
            {
                var isAvailable = !bookedSeatIds.Contains(seat.SeatId);
                decimal price = seat.SeatClass switch
                {
                    "Economy" => 50000m,
                    "Business" => 100000m,
                    "VIP" => 200000m,
                    _ => 50000m
                };

                return new SeatAvailabilityResponse
                {
                    SeatId = seat.SeatId,
                    SeatNumber = seat.SeatNumber,
                    CarriageNumber = seat.Carriage.CarriageNumber,
                    SeatType = seat.SeatType,
                    SeatClass = seat.SeatClass,
                    Price = price,
                    IsAvailable = isAvailable,
                    status = isAvailable ? "Available" : "Booked"
                    
                };
            })
            .OrderBy(s => s.CarriageNumber)
            .ThenBy(s => s.SeatNumber)
            .ToList();

            return result;
        }
        public Task<List<SeatAvailabilityResponse>> GetAvailableSeatsAsync(int tripId, int seatId)
        {
            throw new NotImplementedException();
        }
        public Task<List<SeatAvailabilityResponse>> GetAvailableSeatsAsync(int tripId)
        {
            throw new NotImplementedException();
        }
    }
}
