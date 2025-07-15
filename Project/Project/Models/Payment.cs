using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Payment")]
[Index("BookingId", Name = "IX_Payment_Booking")]
[Index("PaymentGateway", Name = "IX_Payment_Gateway")]
[Index("Status", Name = "IX_Payment_Status")]
[Index("TransactionId", Name = "IX_Payment_TransactionId")]
[Index("TransactionId", Name = "UQ__Payment__55433A6AEC70FB53", IsUnique = true)]
public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string PaymentMethod { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Amount { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string Currency { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string TransactionId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string GatewayTransactionId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string PaymentGateway { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaymentTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ConfirmedTime { get; set; }

    [StringLength(255)]
    public string FailureReason { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? RefundAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RefundTime { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("Payments")]
    public virtual Booking Booking { get; set; }
}
