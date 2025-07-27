namespace ProjectView.Models
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string TripCode { get; set; } = string.Empty;
        public string TrainNumber { get; set; } = string.Empty;

        // Thông tin hành khách
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerPhone { get; set; } = string.Empty;
        public string PassengerEmail { get; set; } = string.Empty;
        public string? PassengerIdCard { get; set; }
        public DateTime? PassengerDateOfBirth { get; set; }

        // Thông tin chuyến tàu
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        // Thông tin ghế
        public string SeatNumber { get; set; } = string.Empty;
        public string CarriageNumber { get; set; } = string.Empty;
        public string SeatClass { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;

        // Thông tin giá và thanh toán
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "VND";
        public string BookingStatus { get; set; } = string.Empty; // Temporary, Confirmed, Cancelled, Expired
        public string? PaymentStatus { get; set; }

        // Thông tin vé
        public string? TicketCode { get; set; }
        public string? TicketStatus { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? ExpirationTime { get; set; }

        // Guest booking info
        public bool IsGuestBooking { get; set; }
        public string? ContactInfo { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Additional info
        public List<string>? Notes { get; set; }
        public bool CanCancel { get; set; }
        public bool CanExtend { get; set; }
        public int? TimeRemainingSeconds { get; set; }
    }
}
