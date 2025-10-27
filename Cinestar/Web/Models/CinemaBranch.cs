using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("CinemaBranch")]
public partial class CinemaBranch
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string BranchID { get; set; } = null!;

    [StringLength(200)]
    public string BranchName { get; set; } = null!;

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? District { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? OpenHours { get; set; }

    [StringLength(255)]
    public string? MapUrl { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("Branch")]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    [InverseProperty("Branch")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("Branch")]
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    [InverseProperty("Branch")]
    public virtual ICollection<WorkShift> WorkShifts { get; set; } = new List<WorkShift>();
}
