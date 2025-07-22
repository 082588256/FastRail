using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string TransactionId { get; set; } = null!;

    public string? GatewayTransactionId { get; set; }

    public string? PaymentGateway { get; set; }

    public string? Status { get; set; }

    public DateTime? PaymentTime { get; set; }

    public DateTime? ConfirmedTime { get; set; }

    public string? FailureReason { get; set; }

    public decimal? RefundAmount { get; set; }

    public DateTime? RefundTime { get; set; }
}
