using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Route
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }
        public decimal? TotalDistance { get; set; }
        public int? EstimatedDuration { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Station DepartureStation { get; set; } = null!;
        public virtual Station ArrivalStation { get; set; } = null!;
        public virtual ICollection<RouteSegment> RouteSegments { get; set; } = new List<RouteSegment>();
        public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public virtual ICollection<Fare> Fares { get; set; } = new List<Fare>();
        public decimal ?FixedFare { get; internal set; } 
    }
}

