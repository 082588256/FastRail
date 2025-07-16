using Project.DTOs;
using Project.Repository.Carriage;
using Project.Repository.Seat;

namespace Project.Services.Seat
{
    public class SeatService : ISeatService
    {
        private readonly ISeatRepository _repository;
        private readonly ICarriageRepository _carriageRepository;

        public SeatService(ISeatRepository repository, ICarriageRepository carriageRepository)
        {
            _repository = repository;
            _carriageRepository = carriageRepository;
        }
        public async Task<(bool Success, string Message, int SeatId)> CreateAsync(SeatDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SeatNumber))
                return (false, "Seat number is required", 0);

            var carriageDto= await _carriageRepository.GetByIdAsync(dto.CarriageId);
            if (carriageDto == null)
            {
                return (false, "Carriage not found", 0);
            }
            

           if(_repository.CheckDuplicateSeatName(dto.SeatNumber, dto.CarriageId).Result)
            {
                return (false, "Duplicate Seat number in same carriage", 0);
            }

            return await _repository.CreateAsync(dto);
        }

        public async Task<bool> DeleteAsync(int id)=> await _repository.DeleteAsync(id);
        

        public async Task<IEnumerable<SeatDto>> GetAllAsync()=> await _repository.GetAllAsync();
        

        public async Task<SeatDto?> GetByIdAsync(int id)=> await GetByIdAsync(id);
        

        public async Task<(bool Success, string Message)> UpdateAsync(int id, SeatDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SeatNumber))
                return (false, "Seat number is required");

            var carriageDto = await _carriageRepository.GetByIdAsync(dto.CarriageId);
            if (carriageDto == null)
            {
                return (false, "Carriage not found");
            }

            if (_repository.CheckDuplicateSeatName(dto.SeatNumber, dto.CarriageId).Result)
            {
                return (false, "Duplicate Seat number in same carriage");
            }

            return await _repository.UpdateAsync(id, dto);
        }
    }
}
