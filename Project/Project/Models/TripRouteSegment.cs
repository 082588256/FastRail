using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("TripRouteSegment")]
[Index("TripId", "Order", Name = "UQ_TripRouteSegment_Trip_Order", IsUnique = true)]
[Index("TripId", "RouteSegmentId", Name = "UQ_TripRouteSegment_Trip_Segment", IsUnique = true)]
public partial class TripRouteSegment
{
    [Key]
    public int TripRouteSegmentId { get; set; }

    public int TripId { get; set; }

    public int RouteSegmentId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DepartureTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ArrivalTime { get; set; }

    public int Order { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActualDepartureTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActualArrivalTime { get; set; }

    public int? DelayMinutes { get; set; }

    [ForeignKey("RouteSegmentId")]
    [InverseProperty("TripRouteSegments")]
    public virtual RouteSegment RouteSegment { get; set; }

    [ForeignKey("TripId")]
    [InverseProperty("TripRouteSegments")]
    public virtual Trip Trip { get; set; }
}
