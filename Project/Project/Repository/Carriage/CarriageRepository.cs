using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Models;

namespace Project.Repository.Carriage
{
    public class CarriageRepository : ICarriageRepository
    {
        private readonly FastRailDbContext _context;
        private readonly IMapper _mapper;

        public CarriageRepository(FastRailDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IEnumerable<CarriageDto>> GetAllAsync()
        {
            var carriages = await _context.Carriage.ToListAsync();
            return _mapper.Map<IEnumerable<CarriageDto>>(carriages);
        }

        public async Task<CarriageDto?> GetByIdAsync(int id)
        {
            var carriage = await _context.Carriage.FindAsync(id);
            return carriage == null ? null : _mapper.Map<CarriageDto>(carriage);
        }

        public async Task<(bool Success, string Message, int CarriageId)> CreateAsync(CarriageDto dto)
        {
            var carriage = _mapper.Map<Models.Carriage>(dto);
            _context.Carriage.Add(carriage);
            await _context.SaveChangesAsync();
            return (true, "Carriage created successfully", carriage.CarriageId);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, CarriageDto dto)
        {
            var carriage = await _context.Carriage.FindAsync(id);
            if (carriage == null)
                return (false, "Carriage not found");

            carriage.CarriageNumber = dto.CarriageNumber;
            carriage.CarriageType = dto.CarriageType.ToString();
            carriage.TrainId = dto.TrainId;
            carriage.TotalSeats = dto.TotalSeats;
            carriage.Order = dto.Order;
            carriage.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return (true, "Carriage updated successfully");
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var carriage = await _context.Carriage.FindAsync(id);
            if (carriage == null) return false;

            _context.Carriage.Remove(carriage);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
