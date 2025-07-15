using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Carriage")]
[Index("TrainId", "CarriageNumber", Name = "UQ_Carriage_Train_Number", IsUnique = true)]
[Index("TrainId", "Order", Name = "UQ_Carriage_Train_Order", IsUnique = true)]
public partial class Carriage
{
    [Key]
    public int CarriageId { get; set; }

    public int TrainId { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string CarriageNumber { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string CarriageType { get; set; }

    public int TotalSeats { get; set; }

    public int Order { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Carriage")]
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    [ForeignKey("TrainId")]
    [InverseProperty("Carriages")]
    public virtual Train Train { get; set; }
}
