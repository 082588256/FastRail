using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Services;
using Project.Services.Metrics;
using Project.Services.Route;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IStatisticService _statisticService;
        public AdminController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _statisticService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("chart-trip")]

        public async Task<IActionResult> GetChartTrip()
        {
            var result = await _statisticService.getTripChart();
            return Ok(result);
        }

        [HttpGet("chart-seat")]
        public async Task<IActionResult> getChartSeat()
        {
            var result = await _statisticService.getSeatPercentage();
            return Ok(result);
        }

    }

    
}