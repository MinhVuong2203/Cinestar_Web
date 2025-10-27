using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[PrimaryKey("TextKey", "LanguageCode")]
[Table("TextTranslation")]
public partial class TextTranslation
{
    [Key]
    [StringLength(100)]
    [Unicode(false)]
    public string TextKey { get; set; } = null!;

    [Key]
    [StringLength(5)]
    [Unicode(false)]
    public string LanguageCode { get; set; } = null!;

    [StringLength(255)]
    public string DisplayText { get; set; } = null!;

    [ForeignKey("LanguageCode")]
    [InverseProperty("TextTranslations")]
    public virtual Language LanguageCodeNavigation { get; set; } = null!;
}
