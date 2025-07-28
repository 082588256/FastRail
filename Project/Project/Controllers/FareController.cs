using Microsoft.AspNetCore.Mvc;
using Project.Services;
using Project.DTOs;
using Project.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class FareController : ControllerBase
{
    private readonly IFareService _fareService;

    public FareController(IFareService fareService)
    {
        _fareService = fareService;
    }

    // GET: /api/fare?routeId=1&segmentId=2&seatClass=Economy&seatType=Soft
    [HttpGet]
    public async Task<IActionResult> GetFare([FromQuery] int routeId, [FromQuery] int segmentId, [FromQuery] string seatClass, [FromQuery] string seatType)
    {
        var fare = await _fareService.GetFareAsync(routeId, segmentId, seatClass, seatType);
        if (fare == null)
            return NotFound(new { message = "Không tìm thấy giá vé phù hợp" }); // Thay đổi thông báo

        return Ok(fare);
    }

    // GET: /api/fare/staff/fares?page=1&pageSize=10
    [HttpGet("staff/fares")]
    public async Task<IActionResult> GetFares([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? routeId = null, [FromQuery] int? segmentId = null, [FromQuery] string seatClass = null, [FromQuery] string seatType = null)
    {
        var fares = await _fareService.GetFaresAsync(page, pageSize, routeId, segmentId, seatClass, seatType);
        return Ok(new
        {
            success = true,
            data = new
            {
                data = fares.Items,
                page,
                totalPages = fares.TotalPages
            }
        });
    }

    // POST: /api/fare?routeId=1&segmentId=2
    [HttpPost]
    public async Task<IActionResult> SetFare([FromQuery] int routeId, [FromQuery] int segmentId, [FromBody] SetFixedFareRequest request)
    {
        var fare = await _fareService.SetFareAsync(routeId, segmentId, request);
        return Ok(fare);
    }
}