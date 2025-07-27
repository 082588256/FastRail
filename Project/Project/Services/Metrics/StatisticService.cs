using Microsoft.EntityFrameworkCore;
using Project.DTOs.StatisticsDTOs;
using System;

namespace Project.Services.Metrics
{
    public class StatisticService : IStatisticService
    {
        private readonly FastRailDbContext _context;

        public StatisticService(FastRailDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;

            var ticketsToday = await _context.Ticket.CountAsync(t => t.CheckInTime == today);
            var revenue = await _context.Ticket
                .Where(t => t.PurchaseTime.Date == today)
                .SumAsync(t => (long?)t.TotalPrice) ?? 0;

            var upcomingTrips = await _context.Trip
                .CountAsync(t => t.DepartureTime > DateTime.Now);

            var emptySeats = await _context.Seat
                .Include(s => s.TicketSegments).Where(s => s.IsActive).CountAsync();

            var totalSeats = await _context.Seat.CountAsync();
            var soldSeats = await _context.TicketSegment
    .Where(ts => ts.Ticket.PurchaseTime.Date == today)
    .Select(ts => ts.SeatId)
    .Distinct()
    .CountAsync();

            return new DashboardStatsDto
            {
                TicketsToday = ticketsToday,
                TodayRevenue = revenue,
                UpcomingTrips = upcomingTrips,
                PassengersOnBoard = 0,
                ActiveRoutes = await _context.Route.CountAsync(r => r.IsActive),
                EmptySeats = emptySeats,
                SeatOccupancyRate = totalSeats == 0 ? 0 : (int)((soldSeats / (double)totalSeats) * 100)
            };
        }

        public async Task<List<SeatPercentageResponse>> getSeatPercentage()
        {
            var totalTrip = await _context.Trip
                .Include(t => t.Tickets)
                .Include(t => t.Train)
                    .ThenInclude(train => train.Carriages)
                        .ThenInclude(carr => carr.Seats)
                        .Select(g => new SeatPercentageResponse
                        {
                            TripName = g.TripName,
                            AvailableSeat = g.Train.Carriages.SelectMany(c => c.Seats).Count() - g.Tickets.Count(),
                            BookedSeat = g.Tickets.Count()
                        }).ToListAsync();
            return totalTrip;
        }

        public async Task<TripChartResponse> getTripChart()
        {
            var result = await _context.Trip.GroupBy(t => t.DepartureTime.Date)
                .Select(g => new { Date = g.Key, count = g.Count() })
                .OrderBy(r => r.Date)
                .ToListAsync();

            return new TripChartResponse { Dates = result.Select(r => r.Date.ToString("yyyy/MM/dd")).ToList(), Counts = result.Select(r => r.count).ToList() };
        }
    }
}
