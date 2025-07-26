using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.DTOs.Standings;
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

        public async Task<List<RevenueByUserResponse>> GetRevenueByUsers()
        {
            var data = await _context.Payment
                                        .Include(p => p.Booking)
                                        .Where(p => p.Status == "Completed" && p.Booking.ConfirmedAt != null)
                                            .GroupBy(p => p.Booking.UserId != null ? "Registered" : "Guest")
                                        .Select(g => new RevenueByUserResponse
                                         {
                                            BookingType = g.Key,
                                            TotalRevenue = (long)(g.Sum(p => (decimal?)p.Amount) ?? 0)
                                        })
                                        .ToListAsync();
            return data;
        }

            public async Task<List<RevenueReportItem>> getRevenueReportItem()
        {

            var payments = await _context.Payment
        .Include(p => p.Booking)
        .Where(p => p.Booking.ConfirmedAt != null && p.Status == "Completed")
        .ToListAsync();

            if (!payments.Any())
                return new List<RevenueReportItem>();

            var grouped = payments
                .GroupBy(p => p.Booking.ConfirmedAt!.Value.Date)
                .Select(g => new RevenueReportItem
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(p => p.Amount),
                    BookingCount = g.Select(p => p.BookingId).Distinct().Count(),
                    GuestBookingCount = g.Count(p => p.Booking.IsGuestBooking),
                    UserBookingCount = g.Count(p => !p.Booking.IsGuestBooking),
                    AverageBookingValue = g.Select(p => p.BookingId).Distinct().Count() == 0
                        ? 0
                        : g.Sum(p => p.Amount) / g.Select(p => p.BookingId).Distinct().Count()
                })
                .OrderBy(r => r.Date)
                .ToList();

            return grouped;
        }

    

    public async Task<List<SeatPercentageResponse>> getSeatPercentage()
        {
            var data = await _context.Trip
        .Select(trip => new
        {
            TripId = trip.TripId,
            TripName = trip.Train.TrainName + " - " + trip.DepartureTime.ToString("dd/MM"),
            TotalSeats = _context.SeatSegment.Count(s => s.TripId == trip.TripId),
            SoldSeats = _context.TicketSegment
                .Where(t => t.Ticket.TripId == trip.TripId && t.Ticket.Status != "Canceled")
                .Select(t => t.SeatId)
                .Distinct()
                .Count()
        })
        .ToListAsync();

            var result = data.Select(d => new SeatPercentageResponse
            {
                TripName= d.TripName,
               SoldSeats= d.SoldSeats,
                AvailableSeat = d.TotalSeats - d.SoldSeats
            }).ToList();

            return result;
        }

        public async Task<List<TopTripResponse>> GetTopTrips()
        {
            var result = await _context.Trip.Select(t => new TopTripResponse
            {
                TripName = t.Train.TrainName + "-" + t.DepartureTime.ToString("dd/MM"),
                BookedSeats = t.Tickets.Count(),
                Revenue = t.Tickets.Sum(t => t.FinalPrice)
            }).OrderByDescending(t=> t.Revenue).Take(10).ToListAsync(); ;

            return result;
        }

        public async Task<TripChartResponse> getTripChart()
        {
            var result = await _context.Trip.GroupBy(t => t.DepartureTime.Date)
                .Select(g => new { Date = g.Key, count = g.Count() })
                .OrderBy(r => r.Date)
                .ToListAsync();

            return new TripChartResponse { Dates = result.Select(r => r.Date.ToString("yyyy/MM/dd")).ToList(), Counts = result.Select(r => r.count).ToList() };
        }

        public async Task<List<RevenueBySeatTypeDto>> GetRevenueBySeatTypeAsync()
        {
            var revenueBySeatType = await _context.TicketSegment
    .Where(ts => ts.Ticket.Status == "Valid")
    .GroupBy(ts => ts.Seat.SeatType)
    .Select(g => new RevenueBySeatTypeDto
    {
        SeatTypeName = g.Key,
        TotalRevenue = g.Sum(ts => ts.SegmentPrice),
        TicketCount = g.Select(ts => ts.TicketId).Distinct().Count()
    })
    .ToListAsync();
            return revenueBySeatType;
        }

    }
}
