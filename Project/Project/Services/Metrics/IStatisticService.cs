using Project.DTOs;

namespace Project.Services.Metrics
{
    public interface IStatisticService{
         Task<DashboardStatsDto> GetDashboardStatsAsync();
}
}
