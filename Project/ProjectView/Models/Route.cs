using System;
using System.Collections.Generic;

namespace ProjectView.Models;

public partial class Route
{
    public int RouteId { get; set; }

    public string RouteName { get; set; } = null!;

    public string RouteCode { get; set; } = null!;

    public int DepartureStationId { get; set; }

    public int ArrivalStationId { get; set; }

    public decimal? TotalDistance { get; set; }

    public int? EstimatedDuration { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Station ArrivalStation { get; set; } = null!;

    public virtual Station DepartureStation { get; set; } = null!;

    public virtual ICollection<Fare> Fares { get; set; } = new List<Fare>();

    public virtual ICollection<RouteSegment> RouteSegments { get; set; } = new List<RouteSegment>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
