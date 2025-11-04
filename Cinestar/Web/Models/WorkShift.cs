using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("WorkShift")]
public partial class WorkShift
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string ShiftID { get; set; } = string.Empty!;

    public Guid EmployeeID { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? BranchID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime StartTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime EndTime { get; set; }

    public double? WorkingHours { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SalaryPerHour { get; set; }

    [StringLength(30)]
    public string? Status { get; set; }

    [ForeignKey("BranchID")]
    [InverseProperty("WorkShifts")]
    public virtual CinemaBranch? Branch { get; set; }

    [ForeignKey("EmployeeID")]
    [InverseProperty("WorkShifts")]
    public virtual Employee Employee { get; set; } = null!;

}
