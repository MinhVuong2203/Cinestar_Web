using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("Invoice")]
public partial class Invoice
{
    [Key]
    public Guid InvoiceID { get; set; }

    public Guid? EmployeeID { get; set; }

    public Guid? CustomerID { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? BranchID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? IssueDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Discount { get; set; }

    [StringLength(30)]
    public string? Status { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("BranchID")]
    [InverseProperty("Invoices")]
    public virtual CinemaBranch? Branch { get; set; }

    [ForeignKey("CustomerID")]
    [InverseProperty("Invoices")]
    public virtual Customer? Customer { get; set; }

    [ForeignKey("EmployeeID")]
    [InverseProperty("Invoices")]
    public virtual Employee? Employee { get; set; }

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoiceProduct> InvoiceProducts { get; set; } = new List<InvoiceProduct>();

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoiceTicket> InvoiceTickets { get; set; } = new List<InvoiceTicket>();

    [InverseProperty("Invoice")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
