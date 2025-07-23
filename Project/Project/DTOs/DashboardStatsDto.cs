namespace Project.DTOs
{
    public class DashboardStatsDto
    {
        public int TicketsToday { get; set; }
        public int PassengersOnBoard { get; set; }
        public int ActiveRoutes { get; set; }
        public long TodayRevenue { get; set; }
        public int UpcomingTrips { get; set; }
        public int EmptySeats { get; set; }
        public int SeatOccupancyRate { get; set; }
    }
}
