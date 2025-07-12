namespace Project.Models
{
    public class Fare
    {
        public int FareId { get; set; }
        public int RouteId { get; set; }
        public int SegmentId { get; set; }
        public string SeatClass { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = "VND";
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Route Route { get; set; } = null!;
        public virtual RouteSegment Segment { get; set; } = null!;
    }
}