using Project.DTOs;

namespace Project.Repository.Train
{
    public interface ITrainRepository
    {
        Task<IEnumerable<TrainDTO>> GetAllAsync();
        Task<TrainDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message, int TrainId)> CreateAsync(TrainDTO dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, TrainDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
