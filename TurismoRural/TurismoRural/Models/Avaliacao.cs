using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TurismoRural.Models;

[Index("CasaID", "UtilizadorID", Name = "Avaliacao_Casa_Utilizador", IsUnique = true)]
public partial class Avaliacao
{
    [Key]
    public int AvaliacaoID { get; set; }

    public int? CasaID { get; set; }

    public int? UtilizadorID { get; set; }

    public byte Nota { get; set; }

    [Unicode(false)]
    public string? Comentario { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DataAvaliacao { get; set; }

    [ForeignKey("CasaID")]
    [InverseProperty("Avaliacao")]
    public virtual Casa? Casa { get; set; }

    [ForeignKey("UtilizadorID")]
    [InverseProperty("Avaliacao")]
    public virtual Utilizador? Utilizador { get; set; }
}

public partial class CriarAvaliacaoDTO
{
	public int? CasaID { get; set; }

	public byte Nota { get; set; }

	public string? Comentario { get; set; }

}

public partial class EditarAvalicaoDTO
{
	public int AvaliacaoID { get; set; }

	public byte Nota { get; set; }

	public string Comentario { get; set; } = String.Empty;

}
