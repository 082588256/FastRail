using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Trip
{
    public int TripId { get; set; }

    public int TrainId { get; set; }

    public int RouteId { get; set; }

    public string TripCode { get; set; } = null!;

    public string? TripName { get; set; }

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public string? Status { get; set; }

    public int? DelayMinutes { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<PriceCalculationLog> PriceCalculationLogs { get; set; } = new List<PriceCalculationLog>();

    public virtual Route Route { get; set; } = null!;

    public virtual ICollection<SeatSegment> SeatSegments { get; set; } = new List<SeatSegment>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual Train Train { get; set; } = null!;

    public virtual ICollection<TripRouteSegment> TripRouteSegments { get; set; } = new List<TripRouteSegment>();
}
