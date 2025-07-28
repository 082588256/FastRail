using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectView.Models;

    public interface IBasicDataService
    {
         Task<List<SelectListItem>> GetStationSelectListAsync();
        Task<List<SelectListItem>> GetTrainSelectListAsync();
        Task<List<SelectListItem>> GetSeatClassesAsync();
    }
    public class BasicDataService(TrainBookingSystemContext context) : IBasicDataService
    {
        private readonly TrainBookingSystemContext _context = context;

        public async Task<List<SelectListItem>> GetStationSelectListAsync()
        {
            return await _context.Stations
                .Select(s => new SelectListItem
                {
                    Value = s.StationId.ToString(),
                    Text = s.StationName
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetTrainSelectListAsync()
        {
            return await _context.Trains
                .Select(t => new SelectListItem
                {
                    Value = t.TrainNumber,
                    Text = $"{t.TrainNumber} - {t.TrainName}"
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetSeatClassesAsync()
        {
            return await _context.Seats
                .Select(sc => new SelectListItem
                {
                    Value = sc.SeatId.ToString(),
                    Text = sc.SeatNumber
                }).ToListAsync();
        }
    }

