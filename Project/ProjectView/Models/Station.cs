using System;
using System.Collections.Generic;

namespace ProjectView.Models;

public partial class Station
{
    public int StationId { get; set; }

    public string StationName { get; set; } = null!;

    public string StationCode { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Province { get; set; } = null!;

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Route> RouteArrivalStations { get; set; } = new List<Route>();

    public virtual ICollection<Route> RouteDepartureStations { get; set; } = new List<Route>();

    public virtual ICollection<RouteSegment> RouteSegmentFromStations { get; set; } = new List<RouteSegment>();

    public virtual ICollection<RouteSegment> RouteSegmentToStations { get; set; } = new List<RouteSegment>();
}
