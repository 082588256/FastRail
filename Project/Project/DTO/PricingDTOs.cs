namespace Project.DTO
{
    namespace Project.DTOs
    {
        public class PriceBreakdownResponse
        {
            public int TripId { get; set; }
            public int SeatId { get; set; }
            public List<int> SegmentIds { get; set; } = new();
            public List<PriceBreakdownItem> Items { get; set; } = new();
            public decimal SubTotal { get; set; }
            public decimal TotalPrice { get; set; }
            public string Currency { get; set; } = "VND";
            public DateTime Timestamp { get; set; }
        }

        public class PriceBreakdownItem
        {
            public int SegmentId { get; set; }
            public string Description { get; set; } = string.Empty;
            public decimal Distance { get; set; }
            public decimal BasePrice { get; set; }
            public decimal FinalPrice { get; set; }
            public string SeatClass { get; set; } = string.Empty;
            public string SeatType { get; set; } = string.Empty;
            public List<PriceAdjustment>? Adjustments { get; set; }
        }

        public class PriceAdjustment
        {
            public string Type { get; set; } = string.Empty; // "SeatType", "Distance", "Promotion"
            public string Description { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public bool IsPercentage { get; set; }
        }

        public class SegmentPriceRequest
        {
            public int TripId { get; set; }
            public int SeatId { get; set; }
            public int SegmentId { get; set; }
        }

        public class SegmentPriceResponse
        {
            public int SegmentId { get; set; }
            public decimal Price { get; set; }
            public string Currency { get; set; } = "VND";
            public string SeatClass { get; set; } = string.Empty;
            public string SeatType { get; set; } = string.Empty;
            public decimal Distance { get; set; }
            public string CalculationMethod { get; set; } = string.Empty; // "Database", "BusinessLogic"
        }
    }
}
