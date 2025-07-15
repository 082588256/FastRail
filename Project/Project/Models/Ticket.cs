using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int TripId { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string? PassengerIdCard { get; set; }
        public string? PassengerPhone { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal FinalPrice { get; set; }
        public DateTime PurchaseTime { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Valid";
        public DateTime? CheckInTime { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Booking Booking { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual Trip Trip { get; set; } = null!;
        public virtual ICollection<TicketSegment> TicketSegments { get; set; } = new List<TicketSegment>();
    }

}

