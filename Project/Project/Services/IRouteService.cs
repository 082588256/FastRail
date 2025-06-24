using Project.DTOs;

namespace Project.Services
{
    public interface IRouteService
    {
        Task<IEnumerable<RouteDTO>> GetAllRoutesAsync();
        Task<RouteDTO?> GetRouteByIdAsync(int id);
        Task<(bool Success, string Message, int RouteId)> CreateRouteAsync(RouteDTO dto);
        Task<(bool Success, string Message)> UpdateRouteAsync(int id, RouteDTO dto);
        Task<bool> DeleteRouteAsync(int id);
    }
}
