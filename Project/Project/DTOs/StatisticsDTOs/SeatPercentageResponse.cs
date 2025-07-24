namespace Project.DTOs.StatisticsDTOs
{
    public class SeatPercentageResponse
    {
        public string TripName { get; set; } = "";

        public int AvailableSeat { get; set; } = 0;

        public int BookedSeat { get; set; } = 0;
    }
}
