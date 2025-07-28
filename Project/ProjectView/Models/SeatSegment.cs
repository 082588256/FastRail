using System;
using System.Collections.Generic;

namespace ProjectView.Models;

public partial class SeatSegment
{
    public int SeatSegmentId { get; set; }

    public int TripId { get; set; }

    public int SeatId { get; set; }

    public int SegmentId { get; set; }

    public int? BookingId { get; set; }

    public string? Status { get; set; }

    public DateTime? ReservedAt { get; set; }

    public DateTime? BookedAt { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual RouteSegment Segment { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;
}
