using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TurismoRural.Models;

public partial class Localidade
{
    [Key]
    public int LocalidadeID { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Nome { get; set; } = null!;

    public int DistritoID { get; set; }

    [InverseProperty("Localidade")]
    public virtual ICollection<CodigoPostal> CodigoPostal { get; set; } = new List<CodigoPostal>();

    [ForeignKey("DistritoID")]
    [InverseProperty("Localidade")]
    public virtual Distrito Distrito { get; set; } = null!;
}
