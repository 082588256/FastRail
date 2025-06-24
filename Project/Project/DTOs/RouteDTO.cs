namespace Project.DTOs
{
    public class RouteDTO
    {
        public int RouteId { get; set; }
        public string? RouteName { get; set; }
        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }
        public List<RouteSegmentDTO> Segments { get; set; } = new();
    }
}
