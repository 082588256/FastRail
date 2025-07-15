using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Models;
using Project.Repository.Trip;
using System.Net.WebSockets;

namespace Project.Services.Trip
{
    public class TripService : ITripService
    {
        private readonly ItripRepository _repo;
        private readonly IMapper _mapper;
        private readonly IValidator<TripDTO> _validator;
        private readonly FastRailDbContext _context;
       

        public TripService(ItripRepository repo, IMapper mapper, IValidator<TripDTO> validator, FastRailDbContext dbContext)
        {
            _repo = repo;
            _mapper = mapper;
            _validator = validator;
            _context = dbContext;
        }
        public async Task<(bool Success, string Message, int TripId)> CreateAsync(TripDTO dto)
        {
            var validation= await _validator.ValidateAsync(dto);
            if(!validation.IsValid)
            {
                return(false,validation.ToString(), 0);
            }

            bool isConflict = await _context.Trips.AnyAsync(t =>
               t.TrainId == dto.TrainId &&
               ((dto.DepartureTime >= t.DepartureTime && dto.DepartureTime < t.ArrivalTime) ||
                (dto.ArrivalTime > t.DepartureTime && dto.ArrivalTime <= t.ArrivalTime)));

            if(isConflict)
            {
                return (false, "Tàu đã có lịch trình khác trong khoảng thời gian này", 0);
            }

            var segments = await _context.RouteSegments
            .Where(s => s.RouteId == dto.RouteId)
            .ToListAsync();

            var stationIds = segments
                .SelectMany(s => new[] { s.FromStationId, s.ToStationId })
                .Distinct()
                .ToList();

            if (!stationIds.Contains(dto.DepartureStationId) || !stationIds.Contains(dto.ArrivalStationId))
                return (false, "Ga đi hoặc ga đến không nằm trong tuyến đã chọn", 0);
            return await _repo.CreateAsync(dto);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<TripDTO>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<TripDTO?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, TripDTO dto)
        {
            var validation= await _validator.ValidateAsync(dto);
            if(!validation.IsValid)
            {
                return (false, validation.ToString());
            }
            return await _repo.UpdateAsync(id, dto);
        }
    }
}
