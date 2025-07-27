// Services/FareService.cs
using Project.DTO;
using Project.Models;

public class FareService : IFareService
{
    private readonly IFareRepository _fareRepository;

    public FareService(IFareRepository fareRepository)
    {
        _fareRepository = fareRepository;
    }

    public async Task<Fare?> GetFareAsync(int routeId, int segmentId, string seatClass, string seatType)
    {
        return await _fareRepository.GetFareAsync(routeId, segmentId, seatClass, seatType);
    }

    public async Task<Fare> SetFareAsync(int routeId, int segmentId, SetFixedFareRequest request)
    {
        var fare = await _fareRepository.GetFareAsync(routeId, segmentId, request.SeatClass, request.SeatType);

        if (fare != null)
        {
            fare.BasePrice = request.BasePrice;
            //fare.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            fare = new Fare
            {
                RouteId = routeId,
                SegmentId = segmentId,
                SeatClass = request.SeatClass,
                SeatType = request.SeatType,
                BasePrice = request.BasePrice,
                Currency = "VND",
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _fareRepository.AddFareAsync(fare);
        }

        await _fareRepository.SaveChangesAsync();
        return fare;
    }

}

    

