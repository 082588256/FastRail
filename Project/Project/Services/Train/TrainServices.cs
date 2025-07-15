using Project.DTOs;
using Project.Repository.Train;

namespace Project.Services.Train
{
    public class TrainServices : ITrainService
    {
        private readonly ITrainRepository _trainRepository;

        public TrainServices(ITrainRepository trainRepository)
        {
            _trainRepository = trainRepository;
        }
        public async Task<(bool success, string message, int id)> CreateTrainAsync(TrainDTO trainDTO)
        {
            var existing = await _trainRepository.GetAllAsync();
            if (existing.Any(t => t.TrainName.Trim().ToLower() == trainDTO.TrainName.Trim().ToLower()))
            {
                return (false, "Train name already exists", 0);
            }
            return await _trainRepository.CreateAsync(trainDTO);
        }

        public Task<bool> DeleteTrainAsync(int id)
        {
            return _trainRepository.DeleteAsync(id);
        }

        public Task<IEnumerable<TrainDTO>> GetAllTrainsAsync()
        {
            return _trainRepository.GetAllAsync();
        }

        public Task<TrainDTO?> GetTrainByIdAsync(int id)
        {
            return _trainRepository.GetByIdAsync(id);
        }

        public async Task<(bool success, string message)> UpdateTrainAsync(TrainDTO trainDTO, int id)
        {
            var existing = await _trainRepository.GetByIdAsync(id);
            if (existing == null)
                return (false, "Train not found");

            if (trainDTO.TotalCarriages < 1)
                return (false, "Train must have at least one carriage");

            return await _trainRepository.UpdateAsync(id, trainDTO);
        }
    }
}
