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
            string? mappedStatus = status switch
            {
                "Valid" => "Valid",
                "Sold" => "Sold",
                "Cancelled" => "Cancelled",
                _ => status
            };

            var tickets = await _ticketRepository.GetTicketsByStatusAsync(mappedStatus);

            return tickets.Select(t => new TicketDto
            {
                TicketId = t.TicketId,
                Status = t.Status,
                TicketCode = t.TicketCode, 
                PurchaseTime = t.PurchaseTime,
                FinalPrice = t.FinalPrice,
                TripId = t.TripId
            });
        }

    }
}
