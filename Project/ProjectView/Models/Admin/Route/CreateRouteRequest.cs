namespace ProjectView.Models.Admin.Route
{
    public class CreateRouteRequest
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }
        public decimal? TotalDistance { get; set; }
        public int? EstimatedDuration { get; set; }
        public bool IsActive { get; set; } = true;

        public List<SegmentDTO> Segments { get; set; } = new();
    }
}
