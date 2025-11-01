using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("Seat")]
[Index("SeatName", "RoomID", Name = "UQ_Seat", IsUnique = true)]
public partial class Seat
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string SeatID { get; set; } = string.Empty!;

    [StringLength(50)]
    public string SeatName { get; set; } = null!;

    [StringLength(50)]
    public string? SeatType { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string RoomID { get; set; } = null!;

    public bool IsDeleted { get; set; }

    [ForeignKey("RoomID")]
    [InverseProperty("Seats")]
    public virtual Room Room { get; set; } = null!;

    [InverseProperty("Seat")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
