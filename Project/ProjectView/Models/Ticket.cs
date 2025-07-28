using System;
using System.Collections.Generic;

namespace ProjectView.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int TripId { get; set; }

    public string TicketCode { get; set; } = null!;

    public string PassengerName { get; set; } = null!;

    public string? PassengerIdCard { get; set; }

    public string? PassengerPhone { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal FinalPrice { get; set; }

    public DateTime? PurchaseTime { get; set; }

    public string? Status { get; set; }

    public DateTime? CheckInTime { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<TicketSegment> TicketSegments { get; set; } = new List<TicketSegment>();

    public virtual Trip Trip { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
