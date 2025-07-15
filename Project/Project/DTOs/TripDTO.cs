namespace Project.DTOs
{
    public class TripDTO
    {
        public int TripId { get; set; }
        public int TrainId { get; set; }
        public int RouteId { get; set; }

        public string ?Status { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }

        public string? TrainName { get; set; }
        public string? RouteName { get; set; }
    }
}
