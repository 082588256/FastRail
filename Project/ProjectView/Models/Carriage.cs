using System;
using System.Collections.Generic;

namespace ProjectView.Models;

public partial class Carriage
{
    public int CarriageId { get; set; }

    public int TrainId { get; set; }

    public string CarriageNumber { get; set; } = null!;

    public string CarriageType { get; set; } = null!;

    public int TotalSeats { get; set; }

    public int Order { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual Train Train { get; set; } = null!;
}
