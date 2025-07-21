using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class TicketSegment
{
    public int TicketSegmentId { get; set; }

    public int TicketId { get; set; }

    public int SegmentId { get; set; }

    public int SeatId { get; set; }

    public decimal SegmentPrice { get; set; }

    public DateTime DepartureTime { get; set; }

    public DateTime? ArrivalTime { get; set; }

    public string? CheckInStatus { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual RouteSegment Segment { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
