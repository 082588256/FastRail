using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Repository.Route;

namespace Project.Services.Route
{
    public class RouteService : IRouteService
    {
        private readonly FastRailDbContext _context;
        private readonly IRouteRepository _routeRepository;
        public RouteService(IRouteRepository repository, FastRailDbContext context)
        {
            _routeRepository = repository;
            _context = context;
        }
        public async Task<(bool Success, string Message, int RouteId)> CreateRouteAsync(RouteDTO dto)
        {
            var duplicateCode = await _routeRepository.checkduplicateRouteCode(dto.RouteCode);
            if (duplicateCode)
            {
                return (false, "Duplicate Route Code", 0);
            }

            decimal totalSegmentDistance = dto.Segments.Sum(s => s.Distance);
            if (totalSegmentDistance < dto.TotalDistance)
            {
                return (false, "Total distance of segments is less than the route length", 0);
            }

            int totalDuration = dto.Segments.Sum(s => s.EstimatedDuration);
            if (dto.EstimatedDuration>0 && totalDuration < dto.EstimatedDuration)
            {
                return (false, "Total duration of segments is less than the estimated route duration", 0);
            }

            var (success, message, routeId) = await _routeRepository.CreateAsync(dto);
            return (success, message, routeId);
        }

        public async Task<bool> DeleteRouteAsync(int id)
        {
            return await _routeRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<RouteDTO>> GetAllRoutesAsync()
        {
            return await _routeRepository.GetAllAsync();
        }

        public async Task<RouteDTO?> GetRouteByIdAsync(int id)
        {
            return await _routeRepository.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message)> UpdateRouteAsync(int id, RouteDTO dto)
        {
            var duplicateCode = await _routeRepository.checkduplicateRouteCode(dto.RouteCode);
            if (duplicateCode)
            {
                return (false, "Duplicate Route Code");
            }

            decimal totalSegmentDistance = dto.Segments.Sum(s => s.Distance);
            if (totalSegmentDistance < dto.TotalDistance)
            {
                return (false, "Total distance of segments is less than the route length");
            }

            int totalDuration = dto.Segments.Sum(s => s.EstimatedDuration);
            if (dto.EstimatedDuration > 0 && totalDuration < dto.EstimatedDuration)
            {
                return (false, "Total duration of segments is less than the estimated route duration");
            }
            return await _routeRepository.UpdateAsync(id, dto);
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
