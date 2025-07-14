using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.DTOs;
using Project.Services.Trip;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;
        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        public async Task<IActionResult> getTrips()
        {
            var trips= await _tripService.GetAllAsync();
            return Ok(trips);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> getTripById([FromRoute] int id)
        {
            var existing = await _tripService.GetByIdAsync(id);
            return existing == null ? NotFound() : Ok(existing);

        }

        [HttpPost]
        public async Task<IActionResult> createTrip([FromBody] TripDTO dto)
        {
            var result= await _tripService.CreateAsync(dto);
            if(result.Success)
            {
                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    tripId = result.TripId
                }); ;
            }
            else
            {
                return BadRequest(result.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> updateTrip([FromRoute] int id, [FromBody] TripDTO dto)
        {
            var existing= await _tripService.GetByIdAsync(id);
            if(existing == null)
            {
                return NotFound();
            }
            else
            {
                var result= await _tripService.UpdateAsync(id, dto);
                if(result.Success)
                {
                    return Ok(new
                    {
                        success = result.Success,
                        message = result.Message,
                        
                    });
                }
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteTrip([FromRoute]int id)
        {
           var result=  await _tripService.DeleteAsync(id);
            if (result)
            {
                return NoContent();
            }
            return NotFound();
        }
    }
}
