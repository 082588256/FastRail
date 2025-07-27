using System;
using System.Collections.Generic;

namespace ProjectView.Models;

/// <summary>
/// Main booking table supporting both user and guest bookings
/// </summary>
public partial class Booking
{
    public int BookingId { get; set; }

    /// <summary>
    /// Nullable for guest bookings. Foreign key to Users table
    /// </summary>
    public int? UserId { get; set; }

    public int TripId { get; set; }

    /// <summary>
    /// Unique booking code for identification and lookup
    /// </summary>
    public string BookingCode { get; set; } = null!;

    public string BookingStatus { get; set; } = null!;

    public string? PaymentStatus { get; set; }

    public decimal TotalPrice { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime? ExpirationTime { get; set; }

    public string PassengerName { get; set; } = null!;

    public string PassengerPhone { get; set; } = null!;

    public string PassengerEmail { get; set; } = null!;

    public string? PassengerIdCard { get; set; }

    public DateOnly? PassengerDateOfBirth { get; set; }

    /// <summary>
    /// Contact information for guest bookings when UserId is null
    /// </summary>
    public string? ContactName { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? InternalNotes { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;
    public virtual Ticket? Ticket { get; set; }
    public virtual User? User { get; set; }
}
