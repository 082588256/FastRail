using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class TicketSegment
    {
        [Key]
        public int TicketSegmentId { get; set; }
        public int TicketId { get; set; }
        public int SegmentId { get; set; }
        public int SeatId { get; set; }
        public decimal SegmentPrice { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string CheckInStatus { get; set; } = "NotCheckedIn";

        // Navigation properties
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual RouteSegment Segment { get; set; } = null!;
        public virtual Seat Seat { get; set; } = null!;
    }

}

