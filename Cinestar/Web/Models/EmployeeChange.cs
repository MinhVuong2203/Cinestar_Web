using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("EmployeeChange")]
public partial class EmployeeChange
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string ChangeID { get; set; } = string.Empty!;

    public Guid EmployeeID { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public DateOnly? BirthDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Username { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? PasswordHash { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [StringLength(30)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ApprovedDate { get; set; }

    [ForeignKey("EmployeeID")]
    [InverseProperty("EmployeeChanges")]
    public virtual Employee Employee { get; set; } = null!;
}
