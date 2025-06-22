using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }

        public User? User { get; set; }
    }

}
