using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Models;

namespace Project.Repository.Route
{
    public class RouteRepository : IRouteRepository
    {
        private readonly FastRailDbContext _context;
        private readonly IMapper _mapper;
        public RouteRepository(FastRailDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<(bool Success, string Message, int RouteId)> CreateAsync(RouteDTO dto)
        {
            var route = _mapper.Map<Models.Route>(dto);

            for (int i = 0; i < route.RouteSegments.Count; i++)
            {
                route.RouteSegments.ElementAt(i).Order = i;
            }
            if (!IsValidSegmentSequence(route))
                return (false, "Segments are invalid or not sequential", 0);

            if (await IsDuplicateRouteAsync(route))
                return (false, "A route with the same path already exists", 0);

            _context.Route.Add(route);
            await _context.SaveChangesAsync();

            return (true, "Created", route.RouteId);
        }

        private async Task<bool> IsDuplicateRouteAsync(Models.Route route, int? excludeRouteId = null)
        {
            var candidates = await _context.Route
                .Include(r => r.RouteSegments.OrderBy(s => s.Order))
                .Where(r =>
                    r.DepartureStationId == route.DepartureStationId &&
                    r.ArrivalStationId == route.ArrivalStationId &&
                    (!excludeRouteId.HasValue || r.RouteId != excludeRouteId.Value))
                .ToListAsync();

            foreach (var existing in candidates)
            {

                var existingSegments = existing.RouteSegments.ToList();
                var routeSegments = route.RouteSegments.ToList();

                if (existingSegments.Count != routeSegments.Count)
                    continue;

                bool same = true;
                for (int i = 0; i < existing.RouteSegments.Count; i++)
                {
                    var a = existingSegments[i];
                    var b = routeSegments[i];
                    if (a.FromStationId != b.FromStationId || a.ToStationId != b.ToStationId)
                    {
                        same = false;
                        break;
                    }
                }

                if (same) return true;
            }

            return false;
        }
        private bool IsValidSegmentSequence(Models.Route route)
        {
            var segments = route.RouteSegments.OrderBy(s => s.Order).ToList();

            if (segments == null || segments.Count == 0) return false;

            var stationSet = new HashSet<int>();
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];

                if (!stationSet.Add(seg.FromStationId)) return false;
                if (i == segments.Count - 1 && !stationSet.Add(seg.ToStationId)) return false;

                if (i > 0 && segments[i - 1].ToStationId != seg.FromStationId)
                    return false;
            }

            if (segments.First().FromStationId != route.DepartureStationId) return false;
            if (segments.Last().ToStationId != route.ArrivalStationId) return false;

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var route = await _context.Route.Include(r => r.RouteSegments)
                .FirstOrDefaultAsync(r => r.RouteId == id);


            if (route == null) return false;
            _context.RouteSegment.RemoveRange(route.RouteSegments);
            _context.Route.Remove(route);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<RouteDTO>> GetAllAsync()
        {
            return await _context.Route
                .Include(r => r.RouteSegments.OrderBy(s => s.Order))
                .ProjectTo<RouteDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<RouteDTO?> GetByIdAsync(int id)
        {
            var route = await _context.Route
                .Include(r => r.RouteSegments.OrderBy(s => s.Order))
                .FirstOrDefaultAsync(r => r.RouteId == id);

            return route == null ? null : _mapper.Map<RouteDTO>(route);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, RouteDTO dto)
        {
            var existing = await _context.Route.Include(r => r.RouteSegments)
               .FirstOrDefaultAsync(r => r.RouteId == id);

            if (existing == null)
                return (false, "Route not found");

            var updated = _mapper.Map<Models.Route>(dto);
            updated.RouteId = id;

            for (int i = 0; i < updated.RouteSegments.Count; i++)
            {
                updated.RouteSegments.ElementAt(i).Order = i;
            }

            if (!IsValidSegmentSequence(updated))
                return (false, "Segments are invalid or not sequential");

            if (await IsDuplicateRouteAsync(updated, excludeRouteId: id))
                return (false, "A route with the same path already exists");

            _context.RouteSegment.RemoveRange(existing.RouteSegments);
            existing.RouteName = updated.RouteName;
            existing.DepartureStationId = updated.DepartureStationId;
            existing.ArrivalStationId = updated.ArrivalStationId;
            existing.RouteSegments = updated.RouteSegments;

            await _context.SaveChangesAsync();
            return (true, "Updated");
        }
    }
}



