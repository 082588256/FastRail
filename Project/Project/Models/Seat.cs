using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Seat")]
[Index("CarriageId", "SeatNumber", Name = "UQ_Seat_Carriage_Number", IsUnique = true)]
public partial class Seat
{
    [Key]
    public int SeatId { get; set; }

    public int CarriageId { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string SeatNumber { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string SeatType { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string SeatClass { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey("CarriageId")]
    [InverseProperty("Seats")]
    public virtual Carriage Carriage { get; set; }

    [InverseProperty("Seat")]
    public virtual ICollection<SeatSegment> SeatSegments { get; set; } = new List<SeatSegment>();

    [InverseProperty("Seat")]
    public virtual ICollection<TicketSegment> TicketSegments { get; set; } = new List<TicketSegment>();
}
