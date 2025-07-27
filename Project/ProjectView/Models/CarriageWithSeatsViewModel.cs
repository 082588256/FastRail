namespace ProjectView.Models
{
    public class CarriageWithSeatsViewModel
    {
        public string CarriageName { get; set; }
        public List<SeatViewModel> SeatList { get; set; }
    }

    public class SeatViewModel
    {
        public int SeatId { get; set; }
        public int CarriageId { get; set; }
        public string SeatNumber { get; set; }
        public bool IsBooked { get; set; }
    }

}