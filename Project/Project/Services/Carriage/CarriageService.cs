using Project.Constants.Enums;
using Project.DTOs;
using Project.Repository.Carriage;
using Project.Repository.Seat;

namespace Project.Services.Carriage
{
    public class CarriageService : ICarriageService
    {
        private readonly ICarriageRepository _repo;
        private readonly ISeatRepository _seatRepository;
        public CarriageService(ICarriageRepository repo, ISeatRepository seatRepository)
        {
            _repo = repo;
            _seatRepository = seatRepository;
        }
        public async Task<(bool Success, string Message, int CarriageId)> CreateAsync(CarriageDto dto)
        {

            var result=  await _repo.CreateAsync(dto);
            if (result.Success)
            {
                int seatCount = dto.CarriageType switch
                {
                    CarriageType.Vip => 20,
                    CarriageType.Standard => 30,
                    CarriageType.Sleeper => 36,
                    CarriageType.SoftSeat => 48,
                    CarriageType.Economic => 48,
                    CarriageType.HardSeat => 48,
                    _ => 36
                };
                var rows = (int)Math.Ceiling(seatCount / 4.0);
                var seatLetters = new[] { "A", "B", "C", "D" };
                var seats = new List<SeatDto>();
                for (int i = 1; i <= rows; i++)
                {
                    foreach (var letter in seatLetters)
                    {
                        if (seats.Count >= seatCount) break;

                        var seatType = dto.CarriageType switch
                        {
                            CarriageType.Vip => SeatType.VIP,
                            CarriageType.Standard => (letter == "A" || letter == "D") ? SeatType.Window : SeatType.Aisle,
                            CarriageType.Sleeper => (i % 2 == 0) ? SeatType.LowerBerth : SeatType.UpperBerth,
                            CarriageType.HardSeat => SeatType.Hard,
                            CarriageType.SoftSeat => SeatType.Soft,
                            _ => SeatType.Standard
                        };

                        seats.Add(new SeatDto
                        {
                            CarriageId = result.CarriageId,
                            SeatName = $"{letter}{i:D2}",
                            Status = false,
                            SeatType = seatType
                        });
                    }
                }

                foreach (var seat in seats)
                {
                    await _seatRepository.CreateAsync(seat);
                }
            }
            else
            {
                return (false, "Failed to create carriage", 0);
            }
            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<CarriageDto>> GetAllAsync()=> await _repo.GetAllAsync();
        

        public async Task<CarriageDto?> GetByIdAsync(int id)=> await _repo.GetByIdAsync(id);
        

        public async Task<(bool Success, string Message)> UpdateAsync(int id, CarriageDto dto)
        {
            return await _repo.UpdateAsync(id, dto);
        }
    }
}
