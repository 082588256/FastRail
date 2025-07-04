using Project.DTOs;

namespace Project.Services.Train
{
    public interface ITrainService
    {
        Task<IEnumerable<TrainDTO>> GetAllTrainsAsync();

        Task<TrainDTO?> GetTrainByIdAsync(int id);

        Task<(bool success,string message, int id )> CreateTrainAsync(TrainDTO trainDTO);

        Task<(bool success, string message)> UpdateTrainAsync(TrainDTO trainDTO, int id);

        Task<bool> DeleteTrainAsync(int id);
    }
}
