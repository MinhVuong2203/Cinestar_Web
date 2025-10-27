using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("Employee")]
[Index("Phone", Name = "UQ__Employee__5C7E359E2F86D62D", IsUnique = true)]
[Index("CCCD", Name = "UQ__Employee__A955A0AA3E66A65F", IsUnique = true)]
[Index("Email", Name = "UQ__Employee__A9D10534B9BFB939", IsUnique = true)]
public partial class Employee
{
    [Key]
    public Guid EmployeeID { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? BranchID { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public DateOnly? BirthDate { get; set; }

    public int? HourWage { get; set; }

    [StringLength(20)]
    public string? CCCD { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(20)]
    public string? Role { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Username { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? PasswordHash { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    public DateOnly? RegisterDate { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("BranchID")]
    [InverseProperty("Employees")]
    public virtual CinemaBranch? Branch { get; set; }

    [InverseProperty("Employee")]
    public virtual ICollection<EmployeeChange> EmployeeChanges { get; set; } = new List<EmployeeChange>();

    [InverseProperty("Employee")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("Employee")]
    public virtual ICollection<WorkShift> WorkShifts { get; set; } = new List<WorkShift>();
}
