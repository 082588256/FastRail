using Microsoft.AspNetCore.Mvc;
using Project.Services;
using Project.DTO; 
namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTickets([FromQuery] string? status)
        {
            var tickets = await _ticketService.GetTicketsByStatusAsync(status);
            return Ok(tickets);
        }
    }
}
