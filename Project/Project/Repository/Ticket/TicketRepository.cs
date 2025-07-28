using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly FastRailDbContext _context;

        public TicketRepository(FastRailDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(string? status)
        {
            var query = _context.Ticket.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                string statusText = status switch
                {
                    "Valid" => "Valid",
                    "Sold" => "Sold",
                    "Cancelled" => "Cancelled",
                    _ => status
                };

                query = query.Where(t => t.Status == statusText);
            }

            return await query.ToListAsync();
        }

    }
}
