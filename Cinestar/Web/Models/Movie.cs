using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("Movie")]
public partial class Movie
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string MovieID { get; set; } = string.Empty!;

    [StringLength(200)]
    [Required(ErrorMessage = "Vui lòng nhập tên phim")]
    public string Title { get; set; } = null!;
    [Required(ErrorMessage = "Vui lòng nhập thời lượng phim")]
    public int? DurationMinutes { get; set; }

    [StringLength(100)]
    public string? Genre { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    [StringLength(50)]
    public string? Sub { get; set; }

    public bool? Dub { get; set; }

    [StringLength(10)]
    public string? AgeLimit { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập ngày khởi chiếu!")]
    [Column(TypeName = "datetime")]
    public DateTime? StartTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndTime { get; set; }

    public string? Description { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [StringLength(200)]
    public string? LinkTrailer { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("Movie")]
    public virtual ICollection<MovieProduct> MovieProducts { get; set; } = new List<MovieProduct>();

    [InverseProperty("Movie")]
    public virtual ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
}
