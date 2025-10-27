using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("InvoiceTicket")]
[Index("InvoiceID", "TicketID", Name = "UQ_Invoice_Ticket", IsUnique = true)]
public partial class InvoiceTicket
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string InvoiceTicketID { get; set; } = null!;

    public Guid InvoiceID { get; set; }

    public Guid TicketID { get; set; }

    public int? Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? UnitPrice { get; set; }

    [ForeignKey("InvoiceID")]
    [InverseProperty("InvoiceTickets")]
    public virtual Invoice Invoice { get; set; } = null!;

    [ForeignKey("TicketID")]
    [InverseProperty("InvoiceTickets")]
    public virtual Ticket Ticket { get; set; } = null!;
}
