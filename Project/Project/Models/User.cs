using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Project.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string PhoneNum { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Notification>? Notifications { get; set; }
        public ICollection<Role>? Roles { get; set; }
    }

}
