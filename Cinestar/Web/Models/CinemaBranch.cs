using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Attributes;

namespace Web.Models;

[Table("CinemaBranch")]
public partial class CinemaBranch
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string BranchID { get; set; } = null!;

    [StringLength(200)]
    [Required(ErrorMessage = "Tên chi nhánh không được để trống")]
    public string BranchName { get; set; } = null!;

    [StringLength(255)]
    [Required(ErrorMessage = "Địa chỉ không được để trống")]
    public string? Address { get; set; }

    [StringLength(100)]
    [RequiredSelect(ErrorMessage = "Vui lòng chọn Tỉnh / Thành phố")]
    public string? City { get; set; }

    [StringLength(100)]
    [RequiredSelect(ErrorMessage = "Vui lòng chọn Quận / Huyện")]
    public string? District { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? MapUrl { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giờ mở cửa")]
    public TimeOnly OpenHour { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giờ đóng cửa")]
    public TimeOnly CloseHour { get; set; }

    [InverseProperty("Branch")]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    [InverseProperty("Branch")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("Branch")]
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    [InverseProperty("Branch")]
    public virtual ICollection<WorkShift> WorkShifts { get; set; } = new List<WorkShift>();
}
