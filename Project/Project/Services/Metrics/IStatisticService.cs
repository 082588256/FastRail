using Project.DTOs;
using Project.DTOs.Standings;
using Project.DTOs.StatisticsDTOs;

namespace Project.Services.Metrics
{
    public interface IStatisticService{
         Task<DashboardStatsDto> GetDashboardStatsAsync();

         Task<TripChartResponse> getTripChart();

        Task<List<SeatPercentageResponse>> getSeatPercentage();

        Task<List<TopTripResponse>> GetTopTrips();

        Task<List<RevenueReportItem>> getRevenueReportItem();

        Task<List<RevenueByUserResponse>> GetRevenueByUsers();

        Task<List<RevenueBySeatTypeDto>> GetRevenueBySeatTypeAsync();

        
}
}
