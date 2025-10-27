using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("InvoiceProduct")]
[Index("InvoiceID", "ProductID", Name = "UQ_Invoice_Product", IsUnique = true)]
public partial class InvoiceProduct
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string InvoiceProductID { get; set; } = null!;

    public Guid InvoiceID { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string ProductID { get; set; } = null!;

    public int? Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? UnitPrice { get; set; }

    [ForeignKey("InvoiceID")]
    [InverseProperty("InvoiceProducts")]
    public virtual Invoice Invoice { get; set; } = null!;

    [ForeignKey("ProductID")]
    [InverseProperty("InvoiceProducts")]
    public virtual Product Product { get; set; } = null!;
}
