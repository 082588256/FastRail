// Repositories/FareRepository.cs
using Microsoft.EntityFrameworkCore;
using Project;
using Project.Models;

public class FareRepository : IFareRepository
{
    private readonly FastRailDbContext _context;

    public FareRepository(FastRailDbContext context)
    {
        _context = context;
    }

    public async Task<Fare?> GetFareAsync(int routeId, int segmentId, string seatClass, string seatType)
    {
        return await _context.Fare.FirstOrDefaultAsync(f =>
            f.RouteId == routeId &&
            f.SegmentId == segmentId &&
            f.SeatClass == seatClass &&
            f.SeatType == seatType &&
            f.IsActive);
    }

    public async Task AddFareAsync(Fare fare)
    {
        _context.Fare.Add(fare);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
