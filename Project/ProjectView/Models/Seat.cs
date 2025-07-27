using System;
using System.Collections.Generic;

namespace ProjectView.Models;

public partial class Seat
{
    public int SeatId { get; set; }

    public int CarriageId { get; set; }

    public string SeatNumber { get; set; } = null!;

    public string SeatType { get; set; } = null!;

    public string SeatClass { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual Carriage Carriage { get; set; } = null!;

    public virtual ICollection<SeatSegment> SeatSegments { get; set; } = new List<SeatSegment>();

    public virtual ICollection<TicketSegment> TicketSegments { get; set; } = new List<TicketSegment>();
}
