namespace Project.DTOs
{
    public class RouteSegmentDTO
    {
        public int SegmentId { get; set; }
        public int FromStationId { get; set; }
        public int ToStationId { get; set; }

        public int Order { get; set; } = 0;
    }
}
