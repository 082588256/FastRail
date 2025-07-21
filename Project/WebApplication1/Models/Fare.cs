using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Fare
{
    public int FareId { get; set; }

    public int RouteId { get; set; }

    public int SegmentId { get; set; }

    public string SeatClass { get; set; } = null!;

    public string SeatType { get; set; } = null!;

    public decimal BasePrice { get; set; }

    public string? Currency { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Route Route { get; set; } = null!;

    public virtual RouteSegment Segment { get; set; } = null!;
}
