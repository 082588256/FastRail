using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("RouteSegment")]
[Index("FromStationId", Name = "IX_RouteSegment_FromStation")]
[Index("RouteId", "Order", Name = "IX_RouteSegment_Order")]
[Index("RouteId", Name = "IX_RouteSegment_Route")]
[Index("ToStationId", Name = "IX_RouteSegment_ToStation")]
[Index("RouteId", "Order", Name = "UQ_RouteSegment_Route_Order", IsUnique = true)]
[Index("RouteId", "FromStationId", "ToStationId", Name = "UQ_RouteSegment_Route_Stations", IsUnique = true)]
public partial class RouteSegment
{
    [Key]
    public int SegmentId { get; set; }

    public int RouteId { get; set; }

    public int FromStationId { get; set; }

    public int ToStationId { get; set; }

    public int Order { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Distance { get; set; }

    public int EstimatedDuration { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Segment")]
    public virtual ICollection<Fare> Fares { get; set; } = new List<Fare>();

    [ForeignKey("FromStationId")]
    [InverseProperty("RouteSegmentFromStations")]
    public virtual Station FromStation { get; set; }

    [InverseProperty("Segment")]
    public virtual ICollection<PriceCalculationLog> PriceCalculationLogs { get; set; } = new List<PriceCalculationLog>();

    [ForeignKey("RouteId")]
    [InverseProperty("RouteSegments")]
    public virtual Route Route { get; set; }

    [InverseProperty("Segment")]
    public virtual ICollection<SeatSegment> SeatSegments { get; set; } = new List<SeatSegment>();

    [InverseProperty("Segment")]
    public virtual ICollection<TicketSegment> TicketSegments { get; set; } = new List<TicketSegment>();

    [ForeignKey("ToStationId")]
    [InverseProperty("RouteSegmentToStations")]
    public virtual Station ToStation { get; set; }

    [InverseProperty("RouteSegment")]
    public virtual ICollection<TripRouteSegment> TripRouteSegments { get; set; } = new List<TripRouteSegment>();
}
