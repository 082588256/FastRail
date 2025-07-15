using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Fare")]
[Index("SeatClass", Name = "IX_Fare_Class")]
[Index("EffectiveFrom", "EffectiveTo", Name = "IX_Fare_EffectiveDate")]
[Index("RouteId", Name = "IX_Fare_Route")]
[Index("SegmentId", Name = "IX_Fare_Segment")]
[Index("RouteId", "SegmentId", "SeatClass", "SeatType", "EffectiveFrom", Name = "UQ_Fare_Route_Segment_Class_Type_Date", IsUnique = true)]
public partial class Fare
{
    [Key]
    public int FareId { get; set; }

    public int RouteId { get; set; }

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

    [StringLength(3)]
    [Unicode(false)]
    public string Currency { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime EffectiveFrom { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EffectiveTo { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("RouteId")]
    [InverseProperty("Fares")]
    public virtual Route Route { get; set; }

    [ForeignKey("SegmentId")]
    [InverseProperty("Fares")]
    public virtual RouteSegment Segment { get; set; }
}
