using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class PriceCalculationLog
    {
        [Key]
        public int LogId { get; set; }
        public int? BookingId { get; set; }
        public int TripId { get; set; }
        public int SegmentId { get; set; }
        public string SeatClass { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public string PricingMethod { get; set; } = string.Empty;
        public string? PricingFactors { get; set; }
        public DateTime CalculationTime { get; set; } = DateTime.UtcNow;
        public int? UserId { get; set; }

        // Navigation properties
        public virtual Booking? Booking { get; set; }
        public virtual Trip Trip { get; set; } = null!;
        public virtual RouteSegment Segment { get; set; } = null!;
        public virtual User? User { get; set; }
    }
}
