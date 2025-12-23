using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TurismoRural.Models;

public partial class Casa_img
{
    [Key]
    public int ImagemID { get; set; }

    public int? CasaID { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string PathImagem { get; set; } = null!;

    [ForeignKey("CasaID")]
    [InverseProperty("Casa_img")]
    public virtual Casa? Casa { get; set; }
}
