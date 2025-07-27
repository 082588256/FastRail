using System;
using System.Collections.Generic;

namespace ProjectView.Models;

public partial class RouteSegment
{
    public int SegmentId { get; set; }

    public int RouteId { get; set; }

    public int FromStationId { get; set; }

    public int ToStationId { get; set; }

    public int Order { get; set; }

    public decimal Distance { get; set; }

    public int EstimatedDuration { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Fare> Fares { get; set; } = new List<Fare>();

    public virtual Station FromStation { get; set; } = null!;

    public virtual ICollection<PriceCalculationLog> PriceCalculationLogs { get; set; } = new List<PriceCalculationLog>();

    public virtual Route Route { get; set; } = null!;

    public virtual ICollection<SeatSegment> SeatSegments { get; set; } = new List<SeatSegment>();

    public virtual ICollection<TicketSegment> TicketSegments { get; set; } = new List<TicketSegment>();

    public virtual Station ToStation { get; set; } = null!;

    public virtual ICollection<TripRouteSegment> TripRouteSegments { get; set; } = new List<TripRouteSegment>();
}
