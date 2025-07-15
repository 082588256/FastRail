using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("SeatSegment")]
[Index("BookingId", Name = "IX_SeatSegment_Booking")]
[Index("SeatId", Name = "IX_SeatSegment_Seat")]
[Index("SegmentId", Name = "IX_SeatSegment_Segment")]
[Index("Status", Name = "IX_SeatSegment_Status")]
[Index("TripId", Name = "IX_SeatSegment_Trip")]
[Index("TripId", "SeatId", "SegmentId", Name = "UQ_SeatSegment_Trip_Seat_Segment", IsUnique = true)]
public partial class SeatSegment
{
    [Key]
    public int SeatSegmentId { get; set; }

    public int TripId { get; set; }

    public int SeatId { get; set; }

    public int SegmentId { get; set; }

    public int? BookingId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReservedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? BookedAt { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("SeatSegments")]
    public virtual Booking Booking { get; set; }

    [ForeignKey("SeatId")]
    [InverseProperty("SeatSegments")]
    public virtual Seat Seat { get; set; }

    [ForeignKey("SegmentId")]
    [InverseProperty("SeatSegments")]
    public virtual RouteSegment Segment { get; set; }

    [ForeignKey("TripId")]
    [InverseProperty("SeatSegments")]
    public virtual Trip Trip { get; set; }
}
