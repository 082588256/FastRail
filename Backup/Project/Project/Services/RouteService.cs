using Microsoft.EntityFrameworkCore;

namespace Project.Services
{
    public interface IRouteService
    {
        Task<List<int>> GetSegmentIdsByRouteAsync(int routeId, int fromStationId, int toStationId);
    }

    // RouteService.cs
    public class RouteService : IRouteService
    {
        private readonly FastRailDbContext _context;

        public RouteService(FastRailDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetSegmentIdsByRouteAsync(int routeId, int fromStationId, int toStationId)
        {
            var segments = await _context.RouteSegment
                .Where(rs => rs.RouteId == routeId)
                .OrderBy(rs => rs.Order)
                .ToListAsync();

            var start = segments.FirstOrDefault(s => s.FromStationId == fromStationId);
            var end = segments.FirstOrDefault(s => s.ToStationId == toStationId);

            if (start == null || end == null || start.Order > end.Order)
                return new List<int>();

            return segments
                .Where(s => s.Order >= start.Order && s.Order <= end.Order)
                .Select(s => s.SegmentId)
                .ToList();
        }
    }
}
