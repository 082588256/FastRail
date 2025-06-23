using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        public int TripId { get; set; }
        public int SeatId { get; set; }
        public DateTime BookingTime { get; set; }
        public string? Status { get; set; }
        public decimal Price { get; set; }
        public string? QRCodeUrl { get; set; }
        public string? CustomerName { get; set; }

        public Trip? Trip { get; set; }
        public Seat? Seat { get; set; }
        public Payment? Payment { get; set; }
        public ICollection<TicketSegment>? TicketSegments { get; set; }
    }

}
