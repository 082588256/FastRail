using Project.DTOs;

namespace Project.Services.Carriage
{
    public interface ICarriageService
    {
        Task<IEnumerable<CarriageDto>> GetAllAsync();
        Task<CarriageDto?> GetByIdAsync(int id);
        Task<(bool Success, string Message, int CarriageId)> CreateAsync(CarriageDto dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, CarriageDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
