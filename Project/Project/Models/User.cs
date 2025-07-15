using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("User")]
[Index("Email", Name = "IX_User_Email")]
[Index("Phone", Name = "IX_User_Phone")]
[Index("Username", Name = "IX_User_Username")]
[Index("Username", Name = "UQ__User__536C85E49CF81E12", IsUnique = true)]
[Index("Email", Name = "UQ__User__A9D1053421064F37", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string Email { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Phone { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string PasswordHash { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Gender { get; set; }

    [StringLength(255)]
    public string Address { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<PriceCalculationLog> PriceCalculationLogs { get; set; } = new List<PriceCalculationLog>();

    [InverseProperty("User")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    [InverseProperty("User")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
