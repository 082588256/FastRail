using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("TicketSegment")]
[Index("TicketId", "SegmentId", Name = "UQ_TicketSegment_Ticket_Segment", IsUnique = true)]
public partial class TicketSegment
{
    [Key]
    public int TicketSegmentId { get; set; }

    public int TicketId { get; set; }

    public int SegmentId { get; set; }

    public int SeatId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal SegmentPrice { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DepartureTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ArrivalTime { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string CheckInStatus { get; set; }

    [ForeignKey("SeatId")]
    [InverseProperty("TicketSegments")]
    public virtual Seat Seat { get; set; }

    [ForeignKey("SegmentId")]
    [InverseProperty("TicketSegments")]
    public virtual RouteSegment Segment { get; set; }

    [ForeignKey("TicketId")]
    [InverseProperty("TicketSegments")]
    public virtual Ticket Ticket { get; set; }
}
