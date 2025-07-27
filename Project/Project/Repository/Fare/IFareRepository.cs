// Repositories/IFareRepository.cs
using Project.Models;

public interface IFareRepository
{
    Task<Fare?> GetFareAsync(int routeId, int segmentId, string seatClass, string seatType);
    Task AddFareAsync(Fare fare);
    Task SaveChangesAsync();
}
