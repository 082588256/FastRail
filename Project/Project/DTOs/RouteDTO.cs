namespace Project.DTOs
{
    public class RouteDTO
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = "";
        public string RouteCode { get; set; } = "";

        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }

        public decimal TotalDistance { get; set; }

        public int EstimatedDuration { get; set; }
        public string DepartureStationName { get; set; } = "";   // 👈 thêm
        public string ArrivalStationName { get; set; } = "";   // 👈 thêm

        public bool IsActive { get; set; }

        public List<RouteSegmentDTO> Segments { get; set; } = new();
    }
}
