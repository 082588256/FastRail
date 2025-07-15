using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("PriceCalculationLog")]
[Index("BookingId", Name = "IX_PriceLog_Booking")]
[Index("CalculationTime", Name = "IX_PriceLog_CalculationTime")]
[Index("TripId", Name = "IX_PriceLog_Trip")]
public partial class PriceCalculationLog
{
    [Key]
    public int LogId { get; set; }

    public int? BookingId { get; set; }

    public int TripId { get; set; }

    public int SegmentId { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string SeatClass { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string SeatType { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal BasePrice { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal FinalPrice { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string PricingMethod { get; set; }

    public string PricingFactors { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CalculationTime { get; set; }

    public int? UserId { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("PriceCalculationLogs")]
    public virtual Booking Booking { get; set; }

    [ForeignKey("SegmentId")]
    [InverseProperty("PriceCalculationLogs")]
    public virtual RouteSegment Segment { get; set; }

    [ForeignKey("TripId")]
    [InverseProperty("PriceCalculationLogs")]
    public virtual Trip Trip { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("PriceCalculationLogs")]
    public virtual User User { get; set; }
}
