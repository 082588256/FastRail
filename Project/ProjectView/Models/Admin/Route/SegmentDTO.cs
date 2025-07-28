namespace ProjectView.Models.Admin.Route
{
    public class SegmentDTO
    {
        public int SegmentId { get; set; }
        public int RouteId { get; set; }
        public int FromStationId { get; set; }
        public int ToStationId { get; set; }
        public int Order { get; set; }
        public decimal? Distance { get; set; }
        public int? EstimatedDuration { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
