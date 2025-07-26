namespace Project.DTOs.StatisticsDTOs
{
    public class RevenueBySeatTypeDto
    {
        public string SeatTypeName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TicketCount { get; set; }
    }
}
