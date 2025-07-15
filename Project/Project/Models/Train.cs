using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Train
    {
        [Key]
        public int TrainId { get; set; }
        public string TrainNumber { get; set; } = string.Empty;
        public string? TrainName { get; set; }
        public string TrainType { get; set; } = string.Empty;
        public int TotalCarriages { get; set; }
        public int? MaxSpeed { get; set; }
        public string? Manufacturer { get; set; }
        public int? YearOfManufacture { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Carriage> Carriages { get; set; } = new List<Carriage>();
        public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }

}

