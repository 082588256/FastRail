using Project.DTO;
using Project.Repositories;
using Project.Models;

namespace Project.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<IEnumerable<TicketDto>> GetTicketsByStatusAsync(string? status)
        {
            var tickets = await _ticketRepository.GetTicketsByStatusAsync(status);

            return tickets.Select(t => new TicketDto
            {
                TicketId = t.TicketId,
                Status = t.Status,
                TripId = t.TripId
            });
        }
    }
}
