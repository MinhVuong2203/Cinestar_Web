using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("MovieProduct")]
[Index("MovieID", "ProductID", Name = "UQ_MovieProduct", IsUnique = true)]
public partial class MovieProduct
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string MovieProductID { get; set; } = string.Empty!;

    [StringLength(10)]
    [Unicode(false)]
    public string MovieID { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string ProductID { get; set; } = null!;

    [StringLength(20)]
    public string? OfferType { get; set; }

    public int? Quantity { get; set; }

    [StringLength(255)]
    public string? Note { get; set; }

    [ForeignKey("MovieID")]
    [InverseProperty("MovieProducts")]
    public virtual Movie Movie { get; set; } = null!;

    [ForeignKey("ProductID")]
    [InverseProperty("MovieProducts")]
    public virtual Product Product { get; set; } = null!;
}
