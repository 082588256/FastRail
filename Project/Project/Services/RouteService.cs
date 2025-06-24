using Project.DTOs;
using Project.Repository.Route;

namespace Project.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRouteRepository _routeRepository;
        public RouteService(IRouteRepository repository)
        {
            _routeRepository = repository;
        }
        public async Task<(bool Success, string Message, int RouteId)> CreateRouteAsync(RouteDTO dto)
        {
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
            return await UpdateRouteAsync(id, dto);
        }
    }
}
