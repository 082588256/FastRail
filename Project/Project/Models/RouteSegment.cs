using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class RouteSegment
    {
        [Key]
        public int SegmentId { get; set; }

        public int RouteId { get; set; }
        public int FromStationId { get; set; }
        public int ToStationId { get; set; }

        public Route? Route { get; set; }
    }

}
