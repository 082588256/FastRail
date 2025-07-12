namespace Project.DTOs
{
    #region Request DTOs

    public class CreateBookingRequest
    {
        
        public int? UserId { get; set; }
        public int TripId { get; set; }
        public int SeatId { get; set; }

        // Thông tin hành khách (bắt buộc cho cả user và guest)
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerPhone { get; set; } = string.Empty;
        public string PassengerEmail { get; set; } = string.Empty;
        public string? PassengerIdCard { get; set; }
        public DateTime? PassengerDateOfBirth { get; set; }

        // Thông tin liên hệ (cho guest booking)
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Guest identifier
        public bool IsGuestBooking => !UserId.HasValue;
    }

    public class GuestBookingLookupRequest
    {
        public string BookingCode { get; set; } = string.Empty;
    
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // CreditCard, Cash, BankTransfer
        public string? CustomerEmail { get; set; }
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? CVV { get; set; }
    }

    public class PriceBreakdownRequest
    {
        public int TripId { get; set; }
        public int SeatId { get; set; }
        public List<int> SegmentIds { get; set; } = new();
    }

    #endregion

    #region Response DTOs

    public class CreateBookingResponse
    {
        public bool Success { get; set; }
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<int>? SegmentIds { get; set; }
        public bool IsGuestBooking { get; set; }

        // Thông tin tra cứu cho guest
        public string? LookupPhone { get; set; }
        public string? LookupEmail { get; set; }
    }

    public class BookingDetailsResponse
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

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public int BookingId { get; set; }
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentTime { get; set; }
        public string? BookingCode { get; set; }
        public string? TicketCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class TimeRemainingResponse
    {
        public bool IsExpired { get; set; }
        public int RemainingSeconds { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TimeDisplay { get; set; } // "05:23" format
    }

    public class UserBookingStatsResponse
    {
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int ExpiredBookings { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageBookingValue { get; set; }
        public DateTime? LastBookingDate { get; set; }
        public DateTime? NextTripDate { get; set; }
        public List<string>? FavoriteRoutes { get; set; }
        public List<string>? PreferredSeatClasses { get; set; }
    }

    #endregion

    #region Trip and Seat DTOs

    public class TripSearchRequest
    {
        public String DepartureStationName { get; set; }
        public String ArrivalStationName { get; set; }
        public DateTime TravelDate { get; set; }
        //public int PassengerCount { get; set; } = 1;
        //public string? SeatClass { get; set; }
        //public string? SeatType { get; set; }
        //public bool IncludeFullTrips { get; set; } = true;
    }

    public class TripSearchResponse
    {
        public int TripId { get; set; }
        public string TripCode { get; set; } = string.Empty;
        public string TrainNumber { get; set; } = string.Empty;
        public string TrainName { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;

        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        //public int TotalSeats { get; set; }
        //public int AvailableSeats { get; set; }
        //public int BookedSeats { get; set; }

        //public decimal MinPrice { get; set; }
        //public decimal MaxPrice { get; set; }
        //public string Currency { get; set; } = "VND";

        //public List<SeatClassInfo>? SeatClassAvailability { get; set; }
        //public bool IsActive { get; set; }
        //public bool HasAvailableSeats => AvailableSeats > 0;
    }

    public class SeatClassInfo
    {
        public string SeatClass { get; set; } = string.Empty;
        public int AvailableSeats { get; set; }
        public decimal Price { get; set; }
    }

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
        public string? Features { get; set; } // "WiFi, Power outlet"
    }

    public class TripDetailsResponse
    {
        public int TripId { get; set; }
        public string TripCode { get; set; } = string.Empty;
        public string TrainNumber { get; set; } = string.Empty;
        public string TrainName { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;

        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public int BookedSeats { get; set; }

        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public List<CarriageInfo>? Carriages { get; set; }
        public List<SeatClassInfo>? SeatClasses { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CarriageInfo
    {
        public int CarriageId { get; set; }
        public string CarriageNumber { get; set; } = string.Empty;
        public string CarriageType { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public List<string>? SeatClasses { get; set; }
    }

    public class TripScheduleResponse
    {
        public DateTime Date { get; set; }
        public List<TripSearchResponse> Trips { get; set; } = new();
        public int TotalTrips { get; set; }
        public int AvailableTrips { get; set; }
    }

    #endregion

    #region Station DTOs

    public class StationResponse
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RouteResponse
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public decimal TotalDistance { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public int DailyTripsCount { get; set; }
    }

    #endregion

    #region User DTOs

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public UserResponse? User { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UserResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    #endregion

    #region Pricing DTOs

    public class PriceCalculationRequest
    {
        public int SeatId { get; set; }
        public int? DepartureStationId { get; set; }
        public int? ArrivalStationId { get; set; }
        public List<int>? SegmentIds { get; set; }
    }

    public class PriceCalculationResponse
    {
        public int TripId { get; set; }
        public int SeatId { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "VND";
        public List<PriceBreakdownItem> PriceBreakdown { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

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

    #endregion

    #region Admin DTOs

    public class DashboardStats
    {
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int TemporaryBookings { get; set; }
        public int ExpiredBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int GuestBookings { get; set; }
        public int UserBookings { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal AverageBookingValue { get; set; }

        public int TodayBookings { get; set; }
        public int ActiveTrips { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class RevenueReportItem
    {
        public DateTime Date { get; set; }
        public int BookingCount { get; set; }
        public int GuestBookingCount { get; set; }
        public int UserBookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageBookingValue { get; set; }
    }

    #endregion
}