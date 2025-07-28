using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 0;
        public string Currency { get; set; } = "VND";
        public string TransactionId { get; set; } = string.Empty;
        public string? GatewayTransactionId { get; set; }
        public string? PaymentGateway { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime PaymentTime { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedTime { get; set; }
        public string? FailureReason { get; set; }
        public decimal? RefundAmount { get; set; }
        public DateTime? RefundTime { get; set; }

        // Navigation properties
        public virtual Booking Booking { get; set; } = null!;
    }

}

