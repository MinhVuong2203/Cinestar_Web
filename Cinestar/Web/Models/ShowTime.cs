using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("ShowTime")]
public partial class ShowTime
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

    public string ShowTimeID { get; set; } = string.Empty!;

    [Column(TypeName = "datetime")]
    public DateTime StartTime { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Price { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    [Required(ErrorMessage = "Tên phim không được để trống!")]
    public string MovieID { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    [Required(ErrorMessage = "Số phòng không được để trống!")]
    public string RoomID { get; set; } = null!;

    public bool IsDeleted { get; set; }

    [ForeignKey("MovieID")]
    [InverseProperty("ShowTimes")]
    public virtual Movie Movie { get; set; } = null!;

    [ForeignKey("RoomID")]
    [InverseProperty("ShowTimes")]
    public virtual Room Room { get; set; } = null!;

    [InverseProperty("ShowTime")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}


