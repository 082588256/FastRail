using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Models;

namespace Project.Repository.Seat
{
    public class SeatRepository : ISeatRepository
    {
        private readonly FastRailDbContext _dbContext;
        private readonly IMapper _mapper;
        public SeatRepository(FastRailDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<IEnumerable<SeatDto>> GetAllAsync()
        {
            var seats = await _dbContext.Seats.ToListAsync();
            return _mapper.Map<IEnumerable<SeatDto>>(seats);
        }

        public async Task<SeatDto?> GetByIdAsync(int id)
        {
            var seat = await _dbContext.Seats.FindAsync(id);
            return seat == null ? null : _mapper.Map<SeatDto>(seat);
        }

        public async Task<(bool Success, string Message, int SeatId)> CreateAsync(SeatDto dto)
        {
            var seat = _mapper.Map<Models.Seat>(dto);
            _dbContext.Seats.Add(seat);
            await _dbContext.SaveChangesAsync();
            return (true, "Seat created successfully", seat.SeatId);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, SeatDto dto)
        {
            var seat = await _dbContext.Seats.FindAsync(id);
            if (seat == null) return (false, "Seat not found");

            seat.SeatName = dto.SeatName;
            seat.CarriageId = dto.CarriageId;
            seat.SeatType = dto.SeatType.ToString();
            seat.Status = dto.Status;

            await _dbContext.SaveChangesAsync();
            return (true, "Seat updated successfully");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var seat = await _dbContext.Seats.FindAsync(id);
            if (seat == null) return false;

            _dbContext.Seats.Remove(seat);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckDuplicateSeatName(string name, int carriageId)
        {
            return await _dbContext.Seats.AnyAsync(s => s.CarriageId == carriageId && s.SeatName.Equals(name));
        }
    }
}
