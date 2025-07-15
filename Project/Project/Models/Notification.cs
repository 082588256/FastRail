using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Notification")]
[Index("CreatedAt", Name = "IX_Notification_CreatedAt")]
[Index("IsRead", Name = "IX_Notification_IsRead")]
[Index("Type", Name = "IX_Notification_Type")]
[Index("UserId", Name = "IX_Notification_User")]
public partial class Notification
{
    [Key]
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Type { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    [StringLength(1000)]
    public string Message { get; set; }

    public bool? IsRead { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Priority { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string RelatedEntityType { get; set; }

    public int? RelatedEntityId { get; set; }

    public bool? EmailSent { get; set; }

    public bool? SmsSent { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReadAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Notifications")]
    public virtual User User { get; set; }
}
