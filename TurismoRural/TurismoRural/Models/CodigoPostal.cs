using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TurismoRural.Models;

public partial class CodigoPostal
{
    [Key]
    [Column("CodigoPostal")]
    [StringLength(10)]
    [Unicode(false)]
    public string CodigoPostal1 { get; set; } = null!;

    public int LocalidadeID { get; set; }

    [InverseProperty("CodigoPostalNavigation")]
    public virtual ICollection<Casa> Casa { get; set; } = new List<Casa>();

    [ForeignKey("LocalidadeID")]
    [InverseProperty("CodigoPostal")]
    public virtual Localidade Localidade { get; set; } = null!;
}
