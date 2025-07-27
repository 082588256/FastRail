//using Microsoft.AspNetCore.Mvc;
//using Project.Models;

//namespace Project.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class FareController : ControllerBase
//    {
//        private readonly FastRailDbContext _context;

//        public FareController(FastRailDbContext context)
//        {
//            _context = context;
//        }

//        // Thiết lập giá vé cố định cho route
//        [HttpPut("route/{id}/fixed-fare")]
//        public async Task<IActionResult> SetFixedFare(int id, [FromBody] decimal inputFare)
//        {
//            var fare = await _context.Fare.FindAsync(id);
//            if (fare == null)
//                return NotFound();

//            Fare.= inputFare;
//            await _context.SaveChangesAsync();
//            return Ok(fare);
//        }

//        // Thiết lập giá vé cho segment
//        [HttpPut("segment/{id}/fare")]
//        public async Task<IActionResult> SetSegmentFare(int id, [FromBody] decimal fare)
//        {
//            var segment = await _context.RouteSegment.FindAsync(id);
//            if (segment == null)
//                return NotFound();

//            segment.SegmentFare = fare;
//            await _context.SaveChangesAsync();
//            return Ok(segment);
//        }

//        // Lấy giá vé cố định
//        [HttpGet("route/{id}/fixed-fare")]
//        public async Task<ActionResult<decimal?>> GetFixedFare(int id)
//        {
//            var route = await _context.Route.FindAsync(id);
//            if (route == null)
//                return NotFound();

//            return Ok(route);
//        }

//        // Lấy giá vé segment
//        [HttpGet("segment/{id}/fare")]
//        public async Task<ActionResult<decimal?>> GetSegmentFare(int id)
//        {
//            var segment = await _context.RouteSegment.FindAsync(id);
//            if (segment == null)
//                return NotFound();

//            return Ok(segment.SegmentFare);
//        }
//    }
//}
