using Microsoft.AspNetCore.Mvc;
using Project.DTOs;
using Project.Services.Route;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly IRouteService _routeService;
        public RouteController(IRouteService routeService) {
            _routeService = routeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoutes()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            return Ok(routes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRouteById(int id)
        {
            var route = await _routeService.GetRouteByIdAsync(id);
            if (route == null) return NotFound();
            return Ok(route);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] RouteDTO routeDTO)
        {
            var result = await _routeService.CreateRouteAsync(routeDTO);
            if (!result.Success) return BadRequest(result.Message);
            return CreatedAtAction(nameof(GetRoutes), new { id = result.RouteId }, routeDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute([FromBody] RouteDTO routeDTO, int id)
        {
            var result= await _routeService.UpdateRouteAsync(id, routeDTO);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var result= await _routeService.DeleteRouteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
