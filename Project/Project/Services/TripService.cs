using Microsoft.EntityFrameworkCore;
using Project.DTOs;

namespace Project.Services
{
    public interface ITripService
    {
        Task<List<TripSearchResponse>> SearchTripsAsync(TripSearchRequest request);
        Task<List<SeatAvailabilityResponse>> GetAvailableSeatsAsync(int tripId);
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
        public async Task<List<SeatAvailabilityResponse>> GetAvailableSeatsAsync(int tripId)
        {
            // Lấy tất cả ghế của chuyến tàu
            var allSeats = await _context.Seat
                .Include(s => s.Carriage)
                .Where(s => s.Carriage.Train.Trips.Any(t => t.TripId == tripId) && s.IsActive)
                .ToListAsync();

            // Lấy ghế đã đặt
            var bookedSeatIds = await _context.SeatSegment
                .Where(ss => ss.TripId == tripId &&
                            (ss.Status == "Booked" || ss.Status == "TemporaryReserved"))
                .Select(ss => ss.SeatId)
                .ToListAsync();

            var result = new List<SeatAvailabilityResponse>();

            foreach (var seat in allSeats)
            {
                if (!bookedSeatIds.Contains(seat.SeatId))
                {
                    // Tính giá đơn giản
                    decimal price = seat.SeatClass switch
                    {
                        "Economy" => 50000m,
                        "Business" => 100000m,
                        "VIP" => 200000m,
                        _ => 50000m
                    };
                    result.Add(new SeatAvailabilityResponse
                    {
                        SeatId = seat.SeatId,
                        SeatNumber = seat.SeatNumber,
                        CarriageNumber = seat.Carriage.CarriageNumber,
                        SeatType = seat.SeatType,
                        SeatClass = seat.SeatClass,
                        Price = price,
                        IsAvailable = true
                    });
                }
            }
            return result.OrderBy(s => s.CarriageNumber).ThenBy(s => s.SeatNumber).ToList();
        }
    }
}
