using Microsoft.EntityFrameworkCore;
using Project.DTO;
using Project.DTO.Project.DTOs;
using Project.Models;

namespace Project.Services
{
    public interface IPricingService
    {
        Task<decimal> CalculateTotalPriceAsync(int seatId, List<int> segmentIds);
        Task<decimal> CalculateSegmentPriceAsync(int tripId, int seatId, int segmentId);
        //Task<PriceBreakdownResponse> GetPriceBreakdownAsync(int tripId, int seatId, List<int> segmentIds);
    }

    public class PricingService : IPricingService
    {
        private readonly FastRailDbContext _context;
        private readonly ILogger<PricingService> _logger;

        public PricingService(FastRailDbContext context, ILogger<PricingService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 💰 Tính giá cho 1 segment cụ thể
        /// </summary>
        public async Task<decimal> CalculateSegmentPriceAsync(int tripId, int seatId, int segmentId)
        {
            try
            {
                _logger.LogInformation("Calculating segment price for trip {TripId}, seat {SeatId}, segment {SegmentId}",
                    tripId, seatId, segmentId);

                // Lấy thông tin ghế
                var seat = await _context.Seat
                    .Include(s => s.Carriage)
                    .FirstOrDefaultAsync(s => s.SeatId == seatId);

                if (seat == null)
                {
                    _logger.LogWarning("Seat {SeatId} not found", seatId);
                    return 0;
                }

                // Lấy thông tin segment
                var segment = await _context.RouteSegment
                    .Include(rs => rs.FromStation)
                    .Include(rs => rs.ToStation)
                    .FirstOrDefaultAsync(rs => rs.SegmentId == segmentId);

                if (segment == null)
                {
                    _logger.LogWarning("Segment {SegmentId} not found", segmentId);
                    return 0;
                }

                // Kiểm tra trip có sử dụng route này không
                var trip = await _context.Trip
                    .Include(t => t.Route)
                    .FirstOrDefaultAsync(t => t.TripId == tripId);

                if (trip == null)
                {
                    _logger.LogWarning("Trip {TripId} not found", tripId);
                    return 0;
                }

                // Tìm giá trong bảng Fare trước
                var fare = await GetFareFromDatabaseAsync(segmentId, seat.SeatClass, seat.SeatType);

                if (fare.HasValue && fare.Value > 0)
                {
                    _logger.LogInformation("Found fare {Price} from database for segment {SegmentId}, seat class {SeatClass}, seat type {SeatType}",
                        fare.Value, segmentId, seat.SeatClass, seat.SeatType);

                    return fare.Value;
                }

                // Nếu không có trong database, tính giá theo logic business
                var calculatedPrice = CalculatePriceByBusinessLogic(seat, segment, trip);

                _logger.LogInformation("Calculated price {Price} for segment {SegmentId}, seat class {SeatClass}, distance {Distance}km",
                    calculatedPrice, segmentId, seat.SeatClass, segment.Distance);

                return calculatedPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating segment price for trip {TripId}, seat {SeatId}, segment {SegmentId}",
                    tripId, seatId, segmentId);
                return 0;
            }
        }

        /// <summary>
        /// 💰 Tính tổng giá cho nhiều segment
        /// </summary>
        public async Task<decimal> CalculateTotalPriceAsync(int seatId, List<int> segmentIds)
        {
            try
            {
                if (segmentIds == null || !segmentIds.Any())
                {
                    _logger.LogWarning("No segments provided for price calculation");
                    return 0;
                }

                var seat = await _context.Seat
                    .Include(s => s.Carriage)
                    .FirstOrDefaultAsync(s => s.SeatId == seatId);

                if (seat == null)
                {
                    _logger.LogWarning("Seat {SeatId} not found", seatId);
                    return 0;
                }

                decimal totalPrice = 0;

                // Tính giá cho từng segment (giả sử cùng 1 trip)
                var firstTrip = await _context.Trip
                    .FirstOrDefaultAsync(t => t.Route.RouteSegments.Any(rs => segmentIds.Contains(rs.SegmentId)));

                if (firstTrip == null)
                {
                    _logger.LogWarning("No trip found for segments {SegmentIds}", string.Join(",", segmentIds));
                    return GetDefaultTotalPrice(seat.SeatClass, segmentIds.Count);
                }

                foreach (var segmentId in segmentIds)
                {
                    var segmentPrice = await CalculateSegmentPriceAsync(firstTrip.TripId, seatId, segmentId);
                    totalPrice += segmentPrice;
                }

                _logger.LogInformation("Calculated total price {TotalPrice} for seat {SeatId} with {SegmentCount} segments",
                    totalPrice, seatId, segmentIds.Count);

                return totalPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total price for seat {SeatId}", seatId);
                return 0;
            }
        }

        /// <summary>
        /// 📊 Lấy chi tiết breakdown giá
        /// </summary>
        //public async Task<PriceBreakdownResponse> GetPriceBreakdownAsync(int tripId, int seatId, List<int> segmentIds)
        //{
        //    try
        //    {
        //        var breakdown = new PriceBreakdownResponse
        //        {
        //            TripId = tripId,
        //            SeatId = seatId,
        //            SegmentIds = segmentIds,
        //            Items = new List<PriceBreakdownItem>(),
        //            Timestamp = DateTime.UtcNow
        //        };

        //        var seat = await _context.Seats
        //            .Include(s => s.Carriage)
        //            .FirstOrDefaultAsync(s => s.SeatId == seatId);

        //        if (seat == null)
        //        {
        //            breakdown.TotalPrice = 0;
        //            breakdown.Currency = "VND";
        //            return breakdown;
        //        }

        //        decimal totalPrice = 0;

        //        foreach (var segmentId in segmentIds)
        //        {
        //            var segmentPrice = await CalculateSegmentPriceAsync(tripId, seatId, segmentId);

        //            var segment = await _context.RouteSegments
        //                .Include(rs => rs.FromStation)
        //                .Include(rs => rs.ToStation)
        //                .FirstOrDefaultAsync(rs => rs.SegmentId == segmentId);

        //            breakdown.Items.Add(new PriceBreakdownItem
        //            {
        //                SegmentId = segmentId,
        //                Description = segment != null ?
        //                    $"{segment.DepartureStation.StationName} → {segment.ArrivalStation.StationName}" :
        //                    $"Segment {segmentId}",
        //                Distance = segment?.Distance ?? 0,
        //                BasePrice = segmentPrice,
        //                FinalPrice = segmentPrice,
        //                SeatClass = seat.SeatClass,
        //                SeatType = seat.SeatType
        //            });

        //            totalPrice += segmentPrice;
        //        }

        //        breakdown.SubTotal = totalPrice;
        //        breakdown.TotalPrice = totalPrice;
        //        breakdown.Currency = "VND";

        //        return breakdown;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting price breakdown for trip {TripId}, seat {SeatId}", tripId, seatId);
        //        return new PriceBreakdownResponse { TotalPrice = 0, Currency = "VND" };
        //    }
        //}

        #region Private Helper Methods

        /// <summary>
        /// Tìm giá trong database Fare table
        /// </summary>
        private async Task<decimal?> GetFareFromDatabaseAsync(int segmentId, string seatClass, string seatType)
        {
            try
            {
                var fare = await _context.Fare
                    .Where(f => f.SegmentId == segmentId
                        && f.SeatClass == seatClass
                        && f.SeatType == seatType
                        && f.IsActive
                        && DateTime.UtcNow >= f.EffectiveFrom
                        && (f.EffectiveTo == null || DateTime.UtcNow <= f.EffectiveTo))
                    .OrderByDescending(f => f.EffectiveFrom)
                    .FirstOrDefaultAsync();

                return fare?.BasePrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fare from database for segment {SegmentId}", segmentId);
                return null;
            }
        }

        /// <summary>
        /// Tính giá theo logic business (backup khi không có trong DB)
        /// </summary>
        private decimal CalculatePriceByBusinessLogic(Models.Seat seat, RouteSegment segment, Models.Trip trip)
        {
            // Base price theo loại ghế
            decimal basePrice = GetBasePriceBySeatClass(seat.SeatClass);

            // Tính theo khoảng cách (VND/km)
            decimal pricePerKm = GetPricePerKmBySeatClass(seat.SeatClass);
            decimal distancePrice = segment.Distance * pricePerKm;

            // Áp dụng hệ số theo loại ghế
            decimal seatTypeMultiplier = GetSeatTypeMultiplier(seat.SeatType);

            // Tính giá cuối
            decimal finalPrice = (basePrice + distancePrice) * seatTypeMultiplier;

            // Làm tròn đến 1000 VND
            return Math.Ceiling(finalPrice / 1000) * 1000;
        }

        /// <summary>
        /// Giá cơ bản theo hạng ghế
        /// </summary>
        private static decimal GetBasePriceBySeatClass(string seatClass)
        {
            return seatClass switch
            {
                "Economy" => 30000m,     // 30k VND base
                "Business" => 60000m,    // 60k VND base
                "VIP" => 120000m,        // 120k VND base
                "FirstClass" => 200000m, // 200k VND base
                _ => 30000m              // Default Economy
            };
        }

        /// <summary>
        /// Giá theo km cho từng hạng ghế
        /// </summary>
        private static decimal GetPricePerKmBySeatClass(string seatClass)
        {
            return seatClass switch
            {
                "Economy" => 800m,       // 800 VND/km
                "Business" => 1200m,     // 1,200 VND/km
                "VIP" => 2000m,          // 2,000 VND/km
                "FirstClass" => 3000m,   // 3,000 VND/km
                _ => 800m                // Default Economy
            };
        }

        /// <summary>
        /// Hệ số nhân theo loại ghế
        /// </summary>
        private static decimal GetSeatTypeMultiplier(string seatType)
        {
            return seatType switch
            {
                "Window" => 1.1m,        // +10% cho ghế cửa sổ
                "Aisle" => 1.05m,        // +5% cho ghế lối đi
                "Middle" => 1.0m,        // Giá gốc cho ghế giữa
                "Table" => 1.15m,        // +15% cho ghế có bàn
                "Sleeper" => 1.5m,       // +50% cho ghế nằm
                _ => 1.0m                // Default
            };
        }

        /// <summary>
        /// Giá mặc định khi không tính được
        /// </summary>
        private static decimal GetDefaultTotalPrice(string seatClass, int segmentCount)
        {
            decimal defaultPricePerSegment = seatClass switch
            {
                "Economy" => 50000m,
                "Business" => 100000m,
                "VIP" => 200000m,
                "FirstClass" => 300000m,
                _ => 50000m
            };

            return defaultPricePerSegment * segmentCount;
        }

        #endregion
    }
}