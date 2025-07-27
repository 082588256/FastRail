
namespace Project.Controllers
{
    public class TicketDetailsResponse
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; }
        public string BookingStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public bool IsGuestBooking { get; set; }
        public string ContactInfo { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string TicketCode { get; set; }
        public string PassengerName { get; set; }
        public string PassengerPhone { get; set; }
        public string PassengerIdCard { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string TripCode { get; set; }
        public string TrainNumber { get; set; }
        public string DepartureStation { get; set; }
        public string ArrivalStation { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public object SeatNumber { get; set; }
        public object CarriageNumber { get; set; }
    }
}