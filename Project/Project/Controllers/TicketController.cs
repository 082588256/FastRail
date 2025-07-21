using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly FastRailDbContext _context;

        public TicketController(FastRailDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách vé theo trạng thái
        // status: 0 = chưa bán, 1 = đã bán, 2 = đã huỷ
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets([FromQuery] String? status)
        {
            var tickets = _context.Ticket.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                tickets = tickets.Where(t => t.Status == status);

            return await tickets.ToListAsync();
        }
    }
}
