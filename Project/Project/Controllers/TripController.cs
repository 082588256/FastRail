using Microsoft.AspNetCore.Mvc;
using Project.DTOs;
using Project.Services;
namespace Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripSearchService;

        public TripsController(ITripService tripSearchService)
        {
            _tripSearchService = tripSearchService;
        }

        [HttpPost("search")]
        public async Task<ActionResult<List<TripSearchResponse>>> SearchTrips([FromBody] TripSearchRequest request)
        {
            var trips = await _tripSearchService.SearchTripsAsync(request);
            return Ok(trips);
        }
        [HttpGet("{tripId}/seats")]
        public async Task<ActionResult<List<SeatAvailabilityResponse>>> GetSeats(
                int tripId,
                int fromStationId,
                int toStationId)
            {
                var seats = await _tripSearchService.GetAvailableSeatsAsync(tripId, fromStationId, toStationId);
                return Ok(seats);
            }
        }
}

