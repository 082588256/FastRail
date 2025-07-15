using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Station")]
[Index("City", Name = "IX_Station_City")]
[Index("Province", Name = "IX_Station_Province")]
[Index("StationCode", Name = "IX_Station_StationCode")]
[Index("StationCode", Name = "UQ__Station__D388561872B61742", IsUnique = true)]
public partial class Station
{
    [Key]
    public int StationId { get; set; }

    [Required]
    [StringLength(100)]
    public string StationName { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string StationCode { get; set; }

    [Required]
    [StringLength(50)]
    public string City { get; set; }

    [Required]
    [StringLength(50)]
    public string Province { get; set; }

    [StringLength(255)]
    public string Address { get; set; }

    [Column(TypeName = "decimal(10, 8)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(11, 8)")]
    public decimal? Longitude { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("ArrivalStation")]
    public virtual ICollection<Route> RouteArrivalStations { get; set; } = new List<Route>();

    [InverseProperty("DepartureStation")]
    public virtual ICollection<Route> RouteDepartureStations { get; set; } = new List<Route>();

    [InverseProperty("FromStation")]
    public virtual ICollection<RouteSegment> RouteSegmentFromStations { get; set; } = new List<RouteSegment>();

    [InverseProperty("ToStation")]
    public virtual ICollection<RouteSegment> RouteSegmentToStations { get; set; } = new List<RouteSegment>();
}
