using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class TripAssignment
    {
        [Key]
        public int TripAssignmentId { get; set; }
        public int TripId { get; set; }
        public int UserId { get; set; }
        public string? Role { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
