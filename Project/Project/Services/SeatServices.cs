using Microsoft.EntityFrameworkCore;
using System;

namespace Project.Services
{
    public interface ISeatService
    {
        Task<bool> IsSeatAvailableForSegmentsAsync(int tripId, int seatId, List<int> segmentIds);
    }

    public class SeatService : ISeatService
    {
        private readonly FastRailDbContext _context;

        public SeatService(FastRailDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsSeatAvailableForSegmentsAsync(int tripId, int seatId, List<int> segmentIds)
        {
            return !await _context.SeatSegment
                .AnyAsync(ss =>
                    ss.TripId == tripId &&
                    ss.SeatId == seatId &&
                    segmentIds.Contains(ss.SegmentId) &&
                    (ss.Status == "TemporaryReserved" || ss.Status == "Booked"));
        }
    }

}
