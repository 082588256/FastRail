namespace ProjectView.Models.Admin.Route
{
    public class UpdateRouteRequest
    {
        public int Id { get; set; }
        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }
        public decimal TotalDistance { get; set; }
        public int EstimatedDuration { get; set; }

        public string DepartureStationName { get; set; } = "";

        public string ArrivalStationName { get; set; } = "";
    }
}
