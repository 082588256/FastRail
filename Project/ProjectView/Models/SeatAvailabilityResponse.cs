namespace ProjectView.Models
{
    public class SeatAvailabilityResponse
    {
        public int SeatId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string CarriageNumber { get; set; } = string.Empty;
        public string CarriageType { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public string SeatClass { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "VND";
        public bool IsAvailable { get; set; }
        public bool IsWindow { get; set; }
        public bool IsAisle { get; set; }
    }
}