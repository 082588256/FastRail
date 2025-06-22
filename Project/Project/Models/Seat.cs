using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Seat
    {
        [Key]
        public int SeatId { get; set; }

        public int CarriageId { get; set; }
        public string? SeatType { get; set; }

        public Carriage? Carriage { get; set; }
    }

}
