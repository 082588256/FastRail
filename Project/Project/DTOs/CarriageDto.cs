using Project.Constants.Enums;

namespace Project.DTOs
{
    public class CarriageDto
    {
        public int CarriageId { get; set; }
        public int TrainId { get; set; }

        public CarriageType CarriageType { get; set; }
        public bool? Status { get; set; }
    }
}
