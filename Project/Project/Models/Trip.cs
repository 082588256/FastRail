using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Trip")]
[Index("TripCode", Name = "IX_Trip_Code")]
[Index("DepartureTime", Name = "IX_Trip_DepartureTime")]
[Index("RouteId", Name = "IX_Trip_Route")]
[Index("Status", Name = "IX_Trip_Status")]
[Index("TrainId", Name = "IX_Trip_Train")]
[Index("TripCode", Name = "UQ__Trip__4992CD16B0D072E1", IsUnique = true)]
public partial class Trip
{
    [Key]
    public int TripId { get; set; }

    public int TrainId { get; set; }

    public int RouteId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string TripCode { get; set; }

    [StringLength(100)]
    public string TripName { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DepartureTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ArrivalTime { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; }

    public int? DelayMinutes { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Trip")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [InverseProperty("Trip")]
    public virtual ICollection<PriceCalculationLog> PriceCalculationLogs { get; set; } = new List<PriceCalculationLog>();

    [ForeignKey("RouteId")]
    [InverseProperty("Trips")]
    public virtual Route Route { get; set; }

    [InverseProperty("Trip")]
    public virtual ICollection<SeatSegment> SeatSegments { get; set; } = new List<SeatSegment>();

    [InverseProperty("Trip")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    [ForeignKey("TrainId")]
    [InverseProperty("Trips")]
    public virtual Train Train { get; set; }

    [InverseProperty("Trip")]
    public virtual ICollection<TripRouteSegment> TripRouteSegments { get; set; } = new List<TripRouteSegment>();
}
