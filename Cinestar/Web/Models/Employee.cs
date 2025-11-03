using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Models;

[Table("Employee")]
[Index("Phone", Name = "UQ__Employee__5C7E359E2F86D62D", IsUnique = true)]
[Index("CCCD", Name = "UQ__Employee__A955A0AA3E66A65F", IsUnique = true)]
[Index("Email", Name = "UQ__Employee__A9D10534B9BFB939", IsUnique = true)]
public partial class Employee
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid EmployeeID { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    [Required(ErrorMessage = "Chi nhánh không được để trống")]
    public string? BranchID { get; set; }

    [StringLength(100)]
    [Required(ErrorMessage = "Họ tên không được để trống")]
    public string FullName { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại chỉ chứa số và phải có 10-11 chữ số")]
    [Remote(action: "CheckPhone", controller: "Employee", AdditionalFields = "EmployeeID", ErrorMessage = "Số điện thoại đã tồn tại")]
    public string? Phone { get; set; }

    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Remote(action: "CheckEmail", controller: "Employee", AdditionalFields = "EmployeeID", ErrorMessage = "Email đã tồn tại")]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [Remote(action: "CheckBirthDate", controller: "Employee", AdditionalFields = "EmployeeID", ErrorMessage = "Chưa đủ tuổi")]
    public DateOnly? BirthDate { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Lương phải lớn hơn 0")]
    public int? HourWage { get; set; }

    [StringLength(20)]
    [Required(ErrorMessage = "CCCD không được để trống")]
    [Remote(action: "CheckCCCD", controller: "Employee", AdditionalFields = "EmployeeID", ErrorMessage = "CCCD đã tồn tại")]
    public string? CCCD { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(20)]
    public string? Role { get; set; }

    [Unicode(false)]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Username phải từ 6-50 ký tự")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])[A-Za-z0-9_]+$", ErrorMessage = "Username phải có chữ hoa, chữ thường, chỉ chứa chữ cái, số và dấu _")]
    [Remote(action: "CheckUsername", controller: "Employee", AdditionalFields = "EmployeeID", ErrorMessage = "Username đã tồn tại")]
    public string? Username { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.")]
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
