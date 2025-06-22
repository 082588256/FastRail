using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class TicketSegment
    {
        [Key]
        public int TicketSegmentId { get; set; }

        public int TicketId { get; set; }
        public int SegmentId { get; set; }

        public Ticket? Ticket { get; set; }
        public RouteSegment? Segment { get; set; }
    }

}
