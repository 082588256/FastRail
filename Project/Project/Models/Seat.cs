using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Seat
    {
        [Key]
        public int SeatId { get; set; }
        public int CarriageId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public string SeatClass { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Carriage Carriage { get; set; } = null!;
        public virtual ICollection<SeatSegment> SeatSegments { get; set; } = new List<SeatSegment>();
        public virtual ICollection<TicketSegment> TicketSegments { get; set; } = new List<TicketSegment>();
    }

}
