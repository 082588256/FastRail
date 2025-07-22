using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Station
    {
        [Key]
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Route> DepartureRoutes { get; set; } = new List<Route>();
        public virtual ICollection<Route> ArrivalRoutes { get; set; } = new List<Route>();
        public virtual ICollection<RouteSegment> FromSegments { get; set; } = new List<RouteSegment>();
        public virtual ICollection<RouteSegment> ToSegments { get; set; } = new List<RouteSegment>();
    }

}
