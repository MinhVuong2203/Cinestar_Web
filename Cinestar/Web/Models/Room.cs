using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("Room")]
[Index("RoomName", Name = "UQ__Room__6B500B553EF8042B", IsUnique = true)]
public partial class Room
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string RoomID { get; set; } = null!;

    [StringLength(100)]
    public string RoomName { get; set; } = null!;

    public int? SeatCount { get; set; }

    public string? Description { get; set; }

    [StringLength(50)]
    public string? RoomType { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? BranchID { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("BranchID")]
    [InverseProperty("Rooms")]
    public virtual CinemaBranch? Branch { get; set; }

    [InverseProperty("Room")]
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    [InverseProperty("Room")]
    public virtual ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
}
