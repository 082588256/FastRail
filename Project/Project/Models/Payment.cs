using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int TicketId { get; set; }
        public decimal Amount { get; set; }
        public string? Method { get; set; }
        public DateTime PaymentTime { get; set; }

        public Ticket? Ticket { get; set; }
    }

}
