using Project.DTO;


namespace Project.Services
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketDto>> GetTicketsByStatusAsync(string? status);
    }
}
