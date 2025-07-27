using System;
using System.Collections.Generic;

namespace ProjectView.Models;

public partial class Train
{
    public int TrainId { get; set; }

    public string TrainNumber { get; set; } = null!;

    public string? TrainName { get; set; }

    public string TrainType { get; set; } = null!;

    public int TotalCarriages { get; set; }

    public int? MaxSpeed { get; set; }

    public string? Manufacturer { get; set; }

    public int? YearOfManufacture { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Carriage> Carriages { get; set; } = new List<Carriage>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
