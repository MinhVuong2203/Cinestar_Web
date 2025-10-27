using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("Payment")]
public partial class Payment
{
    [Key]
    public Guid PaymentID { get; set; }

    public Guid InvoiceID { get; set; }

    [StringLength(50)]
    public string? Method { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Amount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaymentTime { get; set; }

    [ForeignKey("InvoiceID")]
    [InverseProperty("Payments")]
    public virtual Invoice Invoice { get; set; } = null!;
}
