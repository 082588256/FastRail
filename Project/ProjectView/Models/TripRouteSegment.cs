using System;
using System.Collections.Generic;

namespace ProjectView.Models;

public partial class TripRouteSegment
{
    public int TripRouteSegmentId { get; set; }

    public int TripId { get; set; }

    public int RouteSegmentId { get; set; }

    public DateTime DepartureTime { get; set; }

    public DateTime? ArrivalTime { get; set; }

    public int Order { get; set; }

    public DateTime? ActualDepartureTime { get; set; }

    public DateTime? ActualArrivalTime { get; set; }

    public int? DelayMinutes { get; set; }

    public virtual RouteSegment RouteSegment { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;
}
