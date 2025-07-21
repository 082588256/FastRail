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

        [HttpGet("all")]
        public async Task<ActionResult<List<TripSearchResponse>>> GetAllTrips()
        {
            try
            {
                // Create a request to get all trips for today
                var request = new TripSearchRequest
                {
                    DepartureStationName = "", // Empty to get all
                    ArrivalStationName = "",   // Empty to get all
                    TravelDate = DateTime.Today
                };

                var trips = await _tripSearchService.SearchTripsAsync(request);

                // Add debugging information
                var debugInfo = new
                {
                    success = true,
                    data = trips,
                    count = trips.Count,
                    searchDate = request.TravelDate,
                    message = $"Found {trips.Count} trips"
                };

                return Ok(debugInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}

