namespace ProjectView.Models
{
    public class BookingSuccessViewModel
    {
        public Booking Booking { get; set; }
        public Ticket Ticket { get; set; }
        public List<Ticket> Tickets { get; set; }
    }
}
