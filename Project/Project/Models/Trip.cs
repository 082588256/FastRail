using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }
        public int TrainId { get; set; }
        public int RouteId { get; set; }
        public string TripCode { get; set; } = string.Empty;
        public string? TripName { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = "Scheduled";
        public int DelayMinutes { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Train Train { get; set; } = null!;
        public virtual Route Route { get; set; } = null!;
        public virtual ICollection<TripRouteSegment> TripRouteSegments { get; set; } = new List<TripRouteSegment>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public virtual ICollection<SeatSegment> SeatSegments { get; set; } = new List<SeatSegment>();
    }

    public class TripRouteSegment

    {
        [Key]
        public int TripRouteSegmentId { get; set; }
        public int TripId { get; set; }
        public int RouteSegmentId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public int Order { get; set; }
        public DateTime? ActualDepartureTime { get; set; }
        public DateTime? ActualArrivalTime { get; set; }
        public int DelayMinutes { get; set; } = 0;

        // Navigation properties
        public virtual Trip Trip { get; set; } = null!;
        public virtual RouteSegment RouteSegment { get; set; } = null!;
    }

}

