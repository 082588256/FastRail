using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class SeatSegment
    {
        [Key]
        public int SeatSegmentId { get; set; }
        public int TripId { get; set; }
        public int SeatId { get; set; }
        public int SegmentId { get; set; }
        public int? BookingId { get; set; }
        public string Status { get; set; } = "Available";
        public DateTime? ReservedAt { get; set; }
        public DateTime? BookedAt { get; set; }

        // Navigation properties
        public virtual Trip Trip { get; set; } = null!;
        public virtual Seat Seat { get; set; } = null!;
        public virtual RouteSegment Segment { get; set; } = null!;
        public virtual Booking? Booking { get; set; }
    }
}
