using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Role")]
[Index("RoleName", Name = "UQ__Role__8A2B6160C7716B96", IsUnique = true)]
public partial class Role
{
    [Key]
    public int RoleId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string RoleName { get; set; }

    [StringLength(255)]
    public string Description { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
