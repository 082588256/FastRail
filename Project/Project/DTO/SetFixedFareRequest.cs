namespace Project.DTO
{
    public class SetFixedFareRequest
    {
        public string SeatClass { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
    }

}
