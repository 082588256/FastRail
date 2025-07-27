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

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            return await query.ToListAsync();
        }
    }
}
