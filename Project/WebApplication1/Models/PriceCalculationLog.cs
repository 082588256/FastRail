using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class PriceCalculationLog
{
    public int LogId { get; set; }

    public int? BookingId { get; set; }

    public int TripId { get; set; }

    public int SegmentId { get; set; }

    public string SeatClass { get; set; } = null!;

    public string SeatType { get; set; } = null!;

    public decimal BasePrice { get; set; }

    public decimal FinalPrice { get; set; }

    public string PricingMethod { get; set; } = null!;

    public string? PricingFactors { get; set; }

    public DateTime? CalculationTime { get; set; }

    public int? UserId { get; set; }

    public virtual RouteSegment Segment { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;

    public virtual User? User { get; set; }
}
