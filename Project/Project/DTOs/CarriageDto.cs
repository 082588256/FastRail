using Project.Constants.Enums;

namespace Project.DTOs
{
    public class CarriageDto
    {
        public int CarriageId { get; set; }
        public int TrainId { get; set; }
        public string CarriageNumber { get; set; } = string.Empty;
        public CarriageType CarriageType { get; set; }
        public int TotalSeats { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
