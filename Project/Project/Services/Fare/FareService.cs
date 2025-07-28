using Microsoft.EntityFrameworkCore;
using Project.DTO;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public async Task<(List<Fare> Items, int TotalPages)> GetFaresAsync(int page, int pageSize, int? routeId, int? segmentId, string seatClass, string seatType)
    {
        var query = _fareRepository.GetFaresQueryable();

        if (routeId.HasValue)
            query = query.Where(f => f.RouteId == routeId.Value);
        if (segmentId.HasValue)
            query = query.Where(f => f.SegmentId == segmentId.Value);
        if (!string.IsNullOrEmpty(seatClass))
            query = query.Where(f => f.SeatClass == seatClass);
        if (!string.IsNullOrEmpty(seatType))
            query = query.Where(f => f.SeatType == seatType);

        query = query.Where(f => f.IsActive);

        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalPages);
    }
}