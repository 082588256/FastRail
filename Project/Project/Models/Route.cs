using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Route
    {
        [Key]
        public int RouteId { get; set; }

        public string? RouteName { get; set; }
        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }

        public ICollection<RouteSegment>? Segments { get; set; }

        public ICollection<Trip>? Trips { get; set; } = new List<Trip>();
    }

}
