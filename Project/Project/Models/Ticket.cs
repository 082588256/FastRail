using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Ticket")]
[Index("BookingId", Name = "IX_Ticket_Booking")]
[Index("TicketCode", Name = "IX_Ticket_Code")]
[Index("Status", Name = "IX_Ticket_Status")]
[Index("TripId", Name = "IX_Ticket_Trip")]
[Index("UserId", Name = "IX_Ticket_User")]
[Index("TicketCode", Name = "UQ__Ticket__598CF7A3F94F200F", IsUnique = true)]
public partial class Ticket
{
    [Key]
    public int TicketId { get; set; }

    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int TripId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string TicketCode { get; set; }

    [Required]
    [StringLength(100)]
    public string PassengerName { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string PassengerIdCard { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string PassengerPhone { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalPrice { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? DiscountAmount { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal FinalPrice { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PurchaseTime { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CheckInTime { get; set; }

    [StringLength(500)]
    public string Notes { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("Tickets")]
    public virtual Booking Booking { get; set; }

    [InverseProperty("Ticket")]
    public virtual ICollection<TicketSegment> TicketSegments { get; set; } = new List<TicketSegment>();

    [ForeignKey("TripId")]
    [InverseProperty("Tickets")]
    public virtual Trip Trip { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Tickets")]
    public virtual User User { get; set; }
}
