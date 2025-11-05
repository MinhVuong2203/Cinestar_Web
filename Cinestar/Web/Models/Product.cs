using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("Product")]
public partial class Product
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string ProductID { get; set; } = string.Empty!;

    [StringLength(100)]
    public string ProductName { get; set; } = null!;

    [StringLength(50)]
    public string? ProductType { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Price { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<InvoiceProduct> InvoiceProducts { get; set; } = new List<InvoiceProduct>();

    [InverseProperty("Product")]
    public virtual ICollection<MovieProduct> MovieProducts { get; set; } = new List<MovieProduct>();
}
