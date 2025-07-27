namespace ProjectView.Models
{
    public class TripDetailViewModel
    {
        public int TripId { get; set; }
        public string TripCode { get; set; } = string.Empty;

        public string TrainNumber { get; set; } = string.Empty;
        public string TrainName { get; set; } = string.Empty;

        public string RouteName { get; set; } = string.Empty;
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;

        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public int EstimatedDurationMinutes { get; set; }
    }
}
