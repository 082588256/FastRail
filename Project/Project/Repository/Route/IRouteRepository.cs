using Project.DTOs;

namespace Project.Repository.Route
{
    public interface IRouteRepository
    {
        Task<IEnumerable<RouteDTO>> GetAllAsync();
        Task<RouteDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message, int RouteId)> CreateAsync(RouteDTO dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, RouteDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
