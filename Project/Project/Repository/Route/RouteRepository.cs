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
        private readonly ILogger<RouteRepository> _logger;
        private readonly IMapper _mapper;
        public RouteRepository(FastRailDbContext context, IMapper mapper, ILogger<RouteRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<(bool Success, string Message, int RouteId)> CreateAsync(RouteDTO dto)
        {
            try
            {
                var route = _mapper.Map<Models.Route>(dto);

                for (int i = 0; i < route.RouteSegments.Count; i++)
                {
                    route.RouteSegments.ElementAt(i).Order = i + 1;
                }
                if (!IsValidSegmentSequence(route))
                    return (false, "Segments are invalid or not sequential", 0);

                if (await IsDuplicateRouteAsync(route))
                    return (false, "A route with the same path already exists", 0);

                _context.Route.Add(route);
                await _context.SaveChangesAsync();

                return (true, "Created", route.RouteId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error when saving changes to database: "+ ex.Message);
                return (false, "Create Failed", 0);
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when createting new route: "+ex.Message);
                return (false, "Create Failed", 0);
            }
            
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
            if (route.RouteSegments == null)
            {
                _logger.LogError("RouteSegments is null");
                return false;
            }

            var segments = route.RouteSegments.OrderBy(s => s.Order).ToList();

            if (segments.Count == 0)
            {
                _logger.LogError("No segments provided");
                return false;
            }

            var stationSet = new HashSet<int>();

            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];

                if (!stationSet.Add(seg.FromStationId))
                {
                    _logger.LogError($"Duplicate FromStationId detected at index {i}: {seg.FromStationId}");
                    return false;
                }

                if (i == segments.Count - 1 && !stationSet.Add(seg.ToStationId))
                {
                    _logger.LogError($"Duplicate ToStationId at last segment: {seg.ToStationId}");
                    return false;
                }

                if (i > 0 && segments[i - 1].ToStationId != seg.FromStationId)
                {
                    _logger.LogError($"Segment discontinuity between segment {i - 1} and {i}: " +
                        $"Expected FromStationId={segments[i - 1].ToStationId}, but got {seg.FromStationId}");
                    return false;
                }
            }

            if (segments.First().FromStationId != route.DepartureStationId)
            {
                _logger.LogError($" First segment's FromStationId ({segments.First().FromStationId}) " +
                    $"does not match DepartureStationId ({route.DepartureStationId})");
                return false;
            }

            if (segments.Last().ToStationId != route.ArrivalStationId)
            {
                _logger.LogError($" Last segment's ToStationId ({segments.Last().ToStationId}) " +
                    $"does not match ArrivalStationId ({route.ArrivalStationId})");
                return false;
            }

            _logger.LogInformation(" Segment sequence is valid.");
            return true;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var route = await _context.Route.Include(r => r.RouteSegments)
                .FirstOrDefaultAsync(r => r.RouteId == id);


                if (route == null) return false;
                _context.RouteSegment.RemoveRange(route.RouteSegments);
                _context.Route.Remove(route);
                await _context.SaveChangesAsync();

                return true;
            }
            catch(DbUpdateException dbEx)
            {
                _logger.LogError($"Error while saving changes after delete route with id: {id} {dbEx.Message}");
                return false;
            }
            
        }

        public async Task<IEnumerable<RouteDTO>> GetAllAsync()
        {
            var routes = await _context.Route
    .Include(r => r.RouteSegments)
        .Include(rs => rs.DepartureStation)
        .Include(rs => rs.ArrivalStation)
    .ToListAsync();

            var result = _mapper.Map<List<RouteDTO>>(routes);
            return result;
        }

        public async Task<RouteDTO?> GetByIdAsync(int id)
        {
            var route = await _context.Route
        .Include(r => r.RouteSegments.OrderBy(s => s.Order))
            .ThenInclude(s => s.FromStation)
        .Include(r => r.RouteSegments)
            .ThenInclude(s => s.ToStation)
        .Include(r => r.DepartureStation)
        .Include(r => r.ArrivalStation)
        .FirstOrDefaultAsync(r => r.RouteId == id);

            return route == null ? null : _mapper.Map<RouteDTO>(route);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, RouteDTO dto)
        {
            try
            {
                var existing = await _context.Route.Include(r => r.RouteSegments)
                   .FirstOrDefaultAsync(r => r.RouteId == id);

                if (existing == null)
                    return (false, "Route not found");

                
                existing.RouteName = dto.RouteName ?? existing.RouteName;
                existing.RouteCode = dto.RouteCode ?? existing.RouteCode;
                existing.DepartureStationId = dto.DepartureStationId;
                existing.ArrivalStationId = dto.ArrivalStationId;
                existing.TotalDistance = dto.TotalDistance;
                existing.EstimatedDuration = dto.EstimatedDuration;
                existing.IsActive = dto.IsActive;

                
                var updated = _mapper.Map<Models.Route>(dto);
                updated.RouteId = id;

                
                for (int i = 0; i < updated.RouteSegments.Count; i++)
                {
                    updated.RouteSegments.ElementAt(i).Order = i + 1;
                }

                
                if (!IsValidSegmentSequence(updated))
                    return (false, "Segments are invalid or not sequential");

                if (await IsDuplicateRouteAsync(updated, excludeRouteId: id))
                    return (false, "A route with the same path already exists");

                
                _context.RouteSegment.RemoveRange(existing.RouteSegments);
                foreach (var seg in updated.RouteSegments)
                {
                    seg.RouteId = existing.RouteId;
                    _context.RouteSegment.Add(seg);
                }

                await _context.SaveChangesAsync();
                return (true, "Updated");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error when updating route with id {RouteId}", id);
                return (false, "Database update failed: " + dbEx.InnerException?.Message ?? dbEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when updating route with id {RouteId}", id);
                return (false, "Unexpected error: " + ex.Message);
            }
        }

        public async Task<bool> checkduplicateRouteCode(string routeCode)
        {
            return await _context.Route
                .AnyAsync(route => route.RouteCode.ToLower() == routeCode.ToLower());
        }
    }
}



