using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("Ticket")]
[Index("ShowTimeID", "SeatID", Name = "UQ_Ticket", IsUnique = true)]
public partial class Ticket
{
    [Key]
    public Guid TicketID { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string ShowTimeID { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string SeatID { get; set; } = null!;

    [StringLength(50)]
    public string? TicketType { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Price { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }
    public Guid? LockedBy { get; set; }  // ID người đang giữ ghế
    public DateTime? LockedAt { get; set; }  // Thời gian lock

    public bool IsDeleted { get; set; }

    [InverseProperty("Ticket")]
    public virtual ICollection<InvoiceTicket> InvoiceTickets { get; set; } = new List<InvoiceTicket>();

    [ForeignKey("SeatID")]
    [InverseProperty("Tickets")]
    public virtual Seat Seat { get; set; } = null!;

    [ForeignKey("ShowTimeID")]
    [InverseProperty("Tickets")]
    public virtual ShowTime ShowTime { get; set; } = null!;
}
