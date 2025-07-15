namespace Project.DTOs
{
    public class RouteSegmentDTO
    {
        public int SegmentId { get; set; }
        public int RouteId { get; set; }
        public int FromStationId { get; set; }
        public int ToStationId { get; set; }
        public int Order { get; set; } = 1;
        public decimal Distance { get; set; }
        public int EstimatedDuration { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
