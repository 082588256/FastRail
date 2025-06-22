using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public Station? DepartureStation { get; set; }
        public Station? ArrivalStation { get; set; }

        public ICollection<Ticket>? Tickets { get; set; }
    }

}
