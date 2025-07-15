using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Train")]
[Index("TrainNumber", Name = "IX_Train_TrainNumber")]
[Index("TrainType", Name = "IX_Train_TrainType")]
[Index("TrainNumber", Name = "UQ__Train__10C2CD2F433021EB", IsUnique = true)]
public partial class Train
{
    [Key]
    public int TrainId { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string TrainNumber { get; set; }

    [StringLength(100)]
    public string TrainName { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string TrainType { get; set; }

    public int TotalCarriages { get; set; }

    public int? MaxSpeed { get; set; }

    [StringLength(100)]
    public string Manufacturer { get; set; }

    public int? YearOfManufacture { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Train")]
    public virtual ICollection<Carriage> Carriages { get; set; } = new List<Carriage>();

    [InverseProperty("Train")]
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
