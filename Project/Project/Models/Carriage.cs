using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Carriage
    {
        [Key]
        public int CarriageId { get; set; }
        public int TrainId { get; set; }
        public string CarriageNumber { get; set; } = string.Empty;
        public string CarriageType { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Train Train { get; set; } = null!;
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
