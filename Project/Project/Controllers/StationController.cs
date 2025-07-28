using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Services.Station;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StationController : ControllerBase
    {
        private IStationService _service;
        private readonly ILogger<StationController> _logger;

        public StationController(IStationService stationService, ILogger<StationController> logger)
        {
           _service = stationService;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Get()
        {
            var result= _service.GetStations();
            return Ok(result);
        }
        
    }
}
