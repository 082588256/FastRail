using Project.DTOs;

namespace Project.Repository.Carriage
{
    public interface ICarriageRepository
    {
        Task<IEnumerable<CarriageDto>> GetAllAsync();
        Task<CarriageDto?> GetByIdAsync(int id);
        Task<(bool Success, string Message, int CarriageId)> CreateAsync(CarriageDto dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, CarriageDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
