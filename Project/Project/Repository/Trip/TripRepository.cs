using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Models;

namespace Project.Repository.Trip
{
    public class TripRepository : ItripRepository
    {
        private readonly FastRailDbContext _context;
        private IMapper _mapper;

        public TripRepository(FastRailDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<(bool Success, string Message, int TripId)> CreateAsync(TripDTO dto)
        {
            var entity = _mapper.Map<Models.Trip>(dto);
            _context.Trip.Add(entity);
            await _context.SaveChangesAsync();
            return (true, "Created", entity.TripId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Trip.FindAsync(id);
            if (entity == null) return false;
            _context.Trip.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TripDTO>> GetAllAsync()
        {
            return await _context.Trip.Include(trip => trip.Train)
                .Include(trip => trip.Route).ProjectTo<TripDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<TripDTO?> GetByIdAsync(int id)
        {
            var trip= await _context.Trip.Include(trip=> trip.Train)
                            .Include(trip=> trip.Route)
                            .FirstOrDefaultAsync(x=> x.TripId==id);
            return trip==null? null: _mapper.Map<TripDTO>(trip);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, TripDTO dto)
        {
            var entity = await _context.Trip.FindAsync(id);
            if (entity == null) return (false, "Trip not found");

            _mapper.Map(dto, entity);
            await _context.SaveChangesAsync();
            return (true, "Updated");
        }
    }
}
