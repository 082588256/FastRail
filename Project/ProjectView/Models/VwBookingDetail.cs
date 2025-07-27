using System;
using System.Collections.Generic;

namespace ProjectView.Models;

public partial class VwBookingDetail
{
    public int BookingId { get; set; }

    public string BookingCode { get; set; } = null!;

    public string BookingStatus { get; set; } = null!;

    public string? PaymentStatus { get; set; }

    public decimal TotalPrice { get; set; }

    public string Currency { get; set; } = null!;

    public string PassengerName { get; set; } = null!;

    public string PassengerPhone { get; set; } = null!;

    public string PassengerEmail { get; set; } = null!;

    public string? ContactName { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public int? UserId { get; set; }

    public string? UserFullName { get; set; }

    public string? UserEmail { get; set; }

    public int TripId { get; set; }

    public string TripCode { get; set; } = null!;

    public string TrainNumber { get; set; } = null!;

    public string RouteName { get; set; } = null!;

    public string DepartureStation { get; set; } = null!;

    public string ArrivalStation { get; set; } = null!;

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? ExpirationTime { get; set; }

    public int IsGuestBooking { get; set; }

    public int IsExpired { get; set; }

    public int CanCancel { get; set; }

    public int? TimeRemainingSeconds { get; set; }
}
