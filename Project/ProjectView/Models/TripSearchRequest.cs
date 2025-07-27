namespace ProjectView.Models
{
    public class TripSearchRequest
    {
        public string DepartureStationName { get; set; } = string.Empty;
        public string ArrivalStationName { get; set; } = string.Empty;
        public DateTime TravelDate { get; set; }
    }

}
