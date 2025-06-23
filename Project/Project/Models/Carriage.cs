using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Carriage
    {
        [Key]
        public int CarriageId { get; set; }

        public int TrainId { get; set; }
        public string? Status { get; set; }

        public Train? Train { get; set; }
        public ICollection<Seat> ?Seats { get; set; }
    }
}
