using Project.DTO;
using Project.DTOs;
using Project.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IFareService
{
    Task<Fare?> GetFareAsync(int routeId, int segmentId, string seatClass, string seatType);
    Task<Fare> SetFareAsync(int routeId, int segmentId, SetFixedFareRequest request);
    Task<(List<Fare> Items, int TotalPages)> GetFaresAsync(int page, int pageSize, int? routeId, int? segmentId, string seatClass, string seatType);
}