using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Web.Models;

[Table("Language")]
public partial class Language
{
    [Key]
    [StringLength(5)]
    [Unicode(false)]
    public string LanguageCode { get; set; } = null!;

    [StringLength(50)]
    public string LanguageName { get; set; } = null!;

    [InverseProperty("LanguageCodeNavigation")]
    public virtual ICollection<TextTranslation> TextTranslations { get; set; } = new List<TextTranslation>();
}
