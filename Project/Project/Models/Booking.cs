using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Booking")]
[Index("BookingCode", Name = "IX_Booking_Code")]
[Index("ExpirationTime", Name = "IX_Booking_ExpirationTime")]
[Index("BookingStatus", Name = "IX_Booking_Status")]
[Index("TripId", Name = "IX_Booking_Trip")]
[Index("UserId", Name = "IX_Booking_User")]
[Index("BookingCode", Name = "UQ__Booking__C6E56BD5070982A8", IsUnique = true)]
public partial class Booking
{
    [Key]
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int TripId { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string BookingCode { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string BookingStatus { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalPrice { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpirationTime { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string PaymentTransactionId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string PaymentMethod { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string PaymentStatus { get; set; }

    [StringLength(500)]
    public string Notes { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ConfirmedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CancelledAt { get; set; }

    [InverseProperty("Booking")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Booking")]
    public virtual ICollection<PriceCalculationLog> PriceCalculationLogs { get; set; } = new List<PriceCalculationLog>();

    [InverseProperty("Booking")]
    public virtual ICollection<SeatSegment> SeatSegments { get; set; } = new List<SeatSegment>();

    [InverseProperty("Booking")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    [ForeignKey("TripId")]
    [InverseProperty("Bookings")]
    public virtual Trip Trip { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Bookings")]
    public virtual User User { get; set; }
}
