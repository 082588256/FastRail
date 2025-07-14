using Project.DTOs;

namespace Project.Repository.Trip
{
    public interface ItripRepository
    {
        Task<IEnumerable<TripDTO>> GetAllAsync();
        Task<TripDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message, int TripId)> CreateAsync(TripDTO dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, TripDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
