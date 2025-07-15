using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Route")]
[Index("ArrivalStationId", Name = "IX_Route_ArrivalStation")]
[Index("RouteCode", Name = "IX_Route_Code")]
[Index("DepartureStationId", Name = "IX_Route_DepartureStation")]
[Index("RouteCode", Name = "UQ__Route__FDC3458519D81F44", IsUnique = true)]
public partial class Route
{
    [Key]
    public int RouteId { get; set; }

    [Required]
    [StringLength(100)]
    public string RouteName { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string RouteCode { get; set; }

    public int DepartureStationId { get; set; }

    public int ArrivalStationId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? TotalDistance { get; set; }

    public int? EstimatedDuration { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("ArrivalStationId")]
    [InverseProperty("RouteArrivalStations")]
    public virtual Station ArrivalStation { get; set; }

    [ForeignKey("DepartureStationId")]
    [InverseProperty("RouteDepartureStations")]
    public virtual Station DepartureStation { get; set; }

    [InverseProperty("Route")]
    public virtual ICollection<Fare> Fares { get; set; } = new List<Fare>();

    [InverseProperty("Route")]
    public virtual ICollection<RouteSegment> RouteSegments { get; set; } = new List<RouteSegment>();

    [InverseProperty("Route")]
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
