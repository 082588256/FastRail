// Controllers/FareController.cs
using Microsoft.AspNetCore.Mvc;
using Project.Services;
using Project.DTOs;
using Project.DTO;

[Route("api/[controller]")]
[ApiController]
public class FareController : ControllerBase
{
    private readonly IFareService _fareService;

    public FareController(IFareService fareService)
    {
        _fareService = fareService;
    }

    // GET: /api/fare/route/1/segment/2/fare?seatClass=Economy&seatType=Soft
    [HttpGet("route/{routeId}/segment/{segmentId}/fare")]
    public async Task<IActionResult> GetFare(int routeId, int segmentId, [FromQuery] string seatClass, [FromQuery] string seatType)
    {
        var fare = await _fareService.GetFareAsync(routeId, segmentId, seatClass, seatType);
        if (fare == null)
            return NotFound("Fare not found.");

        return Ok(fare);
    }

    // POST: /api/fare/route/1/segment/2/fare
    [HttpPost("route/{routeId}/segment/{segmentId}/fare")]
    public async Task<IActionResult> SetFare(int routeId, int segmentId, [FromBody] SetFixedFareRequest request)
    {
        var fare = await _fareService.SetFareAsync(routeId, segmentId, request);
        return Ok(fare);
    }

}
