using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Carriage
    {
        [Key]
        public int CarriageId { get; set; }

        public int TrainId { get; set; }
        public bool? Status { get; set; }

        public string? CarriageType { get; set; }
        public Train? Train { get; set; }
        public ICollection<Seat> ?Seats { get; set; }
    }
}
