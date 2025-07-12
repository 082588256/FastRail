using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public string Priority { get; set; } = "Normal";
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public bool EmailSent { get; set; } = false;
        public bool SmsSent { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }

}
