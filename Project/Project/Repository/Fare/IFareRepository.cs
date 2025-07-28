using Project.Models;
using System.Linq;
using System.Threading.Tasks;

public interface IFareRepository
{
    Task<Fare?> GetFareAsync(int routeId, int segmentId, string seatClass, string seatType);
    Task AddFareAsync(Fare fare);
    Task SaveChangesAsync();
    IQueryable<Fare> GetFaresQueryable();
}