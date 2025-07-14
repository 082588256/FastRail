using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        public int ?TrainId { get; set; }

        public int RouteId { get; set; }

        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }

        public string Status { get; set; } = "Scheduled";
        public Station? DepartureStation { get; set; } // 👈 Navigation
        public Station? ArrivalStation { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public Route? Route { get; set; }
        public ICollection<Ticket>? Tickets { get; set; }

        public Train ?Train { get; set; }
    }

}
