using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TurismoRural.Models;

public partial class Distrito
{
    [Key]
    public int DistritoID { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Nome { get; set; } = null!;

    [InverseProperty("Distrito")]
    public virtual ICollection<Localidade> Localidade { get; set; } = new List<Localidade>();
}
