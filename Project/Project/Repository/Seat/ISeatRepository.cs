using Project.DTOs;

namespace Project.Repository.Seat
{
    public interface ISeatRepository
    {
        Task<IEnumerable<SeatDto>> GetAllAsync();
        Task<SeatDto?> GetByIdAsync(int id);
        Task<(bool Success, string Message, int SeatId)> CreateAsync(SeatDto dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, SeatDto dto);
        Task<bool> DeleteAsync(int id);

        Task<bool> CheckDuplicateSeatName(string name, int carriageId);
    }
}
