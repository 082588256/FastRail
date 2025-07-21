using Project.Constants.Enums;

namespace Project.DTOs
{
    public class SeatDto
    {
        public int SeatId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;

        public int CarriageId { get; set; }
        public SeatType SeatType { get; set; }
        //public string SeatNumber { get; set; }
        public string? SeatName { get; set; }
        public bool? Status { get; set; }
    }
}
