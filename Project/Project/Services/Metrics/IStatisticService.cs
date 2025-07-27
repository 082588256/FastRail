using Project.DTOs.StatisticsDTOs;

namespace Project.Services.Metrics
{
    public interface IStatisticService{
         Task<DashboardStatsDto> GetDashboardStatsAsync();

         Task<TripChartResponse> getTripChart();

        Task<List<SeatPercentageResponse>> getSeatPercentage();
}
}
