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
            // Tìm chuyến tàu theo ngày
            var query = _context.Trip

                .Include(t => t.Train)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DepartureStation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.ArrivalStation)

                .Where(t => t.IsActive);

            // If both station names are empty, get trips for the next 90 days (to include September/October trips)
            if (string.IsNullOrEmpty(request.DepartureStationName) && string.IsNullOrEmpty(request.ArrivalStationName))
            {
                var startDate = DateTime.Today;
                var endDate = DateTime.Today.AddDays(90);
                query = query.Where(t => t.DepartureTime.Date >= startDate && t.DepartureTime.Date <= endDate);
            }
            else
            {
                // Use the specific search date
                query = query.Where(t => t.DepartureTime.Date == searchDate);
            }

            // Only filter by station names if they are provided
            if (!string.IsNullOrEmpty(request.DepartureStationName))
            {
                query = query.Where(t => t.Route.DepartureStation.StationName == request.DepartureStationName);
            }
            
            if (!string.IsNullOrEmpty(request.ArrivalStationName))
            {
                query = query.Where(t => t.Route.ArrivalStation.StationName == request.ArrivalStationName);
            }

            var trips = await query.ToListAsync();
            var result = new List<TripSearchResponse>();
             foreach (var trip in trips)
            {
                //// Đếm ghế trống đơn giản
                //var totalSeats = await _context.Seat
                //    .Where(s => s.Carriage.TrainId == trip.TrainId && s.IsActive)
                //    .CountAsync();

                //var bookedSeats = await _context.SeatSegment
                //    .Where(ss => ss.TripId == trip.TripId &&
                //                (ss.Status == "Booked" || ss.Status == "TemporaryReserved"))
                //    .CountAsync();
                //var availableSeats = totalSeats - bookedSeats;

                result.Add(new TripSearchResponse
                {
                    TripId = trip.TripId,
                    TripCode = trip.TripCode,
                    TrainNumber = trip.Train.TrainNumber,
                    RouteName = trip.Route.RouteName,
                    DepartureStation = trip.Route.DepartureStation.StationName,
                    ArrivalStation = trip.Route.ArrivalStation.StationName,
                    DepartureTime = trip.DepartureTime,
                    ArrivalTime = trip.ArrivalTime,
                    TrainName=trip.Train.TrainName,
                    //AvailableSeats = availableSeats,
                    //MinPrice = 50000, // Giá tối thiểu
                    //MaxPrice = 200000, // Giá tối đa
                    EstimatedDurationMinutes = (int)(trip.ArrivalTime - trip.DepartureTime).TotalMinutes
                });
            }

            return result;
        }

        public async Task<List<SeatAvailabilityResponse>> GetAvailableSeatsAsync(int tripId, int fromStationId, int toStationId)
        {
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
            var toOrder = segments.FirstOrDefault(s => s.ToStationId == toStationId)?.Order ?? -1;

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
                             (ss.Status == "Booked" || ss.Status == "TemporaryReserved"))
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
                    IsAvailable = isAvailable
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
