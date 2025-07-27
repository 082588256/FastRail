using Project.Models;

namespace Project.Repositories
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(string? status);
    }
}
