using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("Customer")]
[Index("Phone", Name = "UQ__Customer__5C7E359EA3533212", IsUnique = true)]
[Index("Email", Name = "UQ__Customer__A9D1053418CD4257", IsUnique = true)]
public partial class Customer
{
    [Key]
    public Guid CustomerID { get; set; }

    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    public DateOnly? BirthDate { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Username { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? PasswordHash { get; set; }

    public DateOnly? RegisterDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Point { get; set; }

    public int? VipLevel { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
