// Services/IFareService.cs
using Project.DTO;
using Project.DTOs;
using Project.Models;

public interface IFareService
{
    Task<Fare?> GetFareAsync(int routeId, int segmentId, string seatClass, string seatType);
    Task<Fare> SetFareAsync(int routeId, int segmentId, SetFixedFareRequest request);
}
