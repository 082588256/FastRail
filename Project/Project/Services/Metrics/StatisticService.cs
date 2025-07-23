using Project.DTOs;
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

            var ticketsToday _context.Ticket.Count(t => t.PurchaseTime == today);
            var revenue = await _context.Tickets
                .Where(t => t.BookedDate.Date == today)
                .SumAsync(t => (long?)t.Price) ?? 0;

            var upcomingTrips = await _context.Trips
                .CountAsync(t => t.DepartureTime > DateTime.Now);

            var onBoardCount = await _context.PassengerTrips
                .CountAsync(p => p.Trip.DepartureTime <= DateTime.Now && p.Trip.ArrivalTime >= DateTime.Now);

            var emptySeats = await _context.Seats
                .CountAsync(s => !s.Tickets.Any(t => t.Trip.DepartureTime.Date == today));

            var totalSeats = await _context.Seats.CountAsync();
            var soldSeats = await _context.Tickets
                .Where(t => t.BookedDate.Date == today)
                .Select(t => t.SeatId)
                .Distinct()
                .CountAsync();

            return new DashboardStatsDto
            {
                TicketsToday = ticketsToday,
                TodayRevenue = revenue,
                UpcomingTrips = upcomingTrips,
                PassengersOnBoard = onBoardCount,
                ActiveRoutes = await _context.Routes.CountAsync(r => r.IsActive),
                EmptySeats = emptySeats,
                SeatOccupancyRate = totalSeats == 0 ? 0 : (int)((soldSeats / (double)totalSeats) * 100)
            };
        }
    {
    }
}
