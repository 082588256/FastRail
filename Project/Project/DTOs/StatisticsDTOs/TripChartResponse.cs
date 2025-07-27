namespace Project.DTOs.StatisticsDTOs
{
    public class TripChartResponse
    {
        public List<string> Dates { get; set; } = new();
        public List<int> Counts { get; set; } = new();
    }
}
