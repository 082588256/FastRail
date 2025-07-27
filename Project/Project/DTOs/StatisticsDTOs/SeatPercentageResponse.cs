namespace Project.DTOs.StatisticsDTOs
{
    public class SeatPercentageResponse
    {
        public string TripName { get; set; } = "";

        public int AvailableSeat { get; set; } = 0;

        public int SoldSeats { get; set; } = 0;
    }
}
