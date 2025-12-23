using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TurismoRural.Models;

public partial class Casa
{
    [Key]
    public int CasaID { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string Titulo { get; set; } = null!;

    [Unicode(false)]
    public string? Descricao { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Tipo { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string Tipologia { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Preco { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Morada { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string CodigoPostal { get; set; } = null!;

    public int UtilizadorID { get; set; }

    [InverseProperty("Casa")]
    public virtual ICollection<Avaliacao> Avaliacao { get; set; } = new List<Avaliacao>();

    [InverseProperty("Casa")]
    public virtual ICollection<Casa_img> Casa_img { get; set; } = new List<Casa_img>();

    [ForeignKey("CodigoPostal")]
    [InverseProperty("Casa")]
    public virtual CodigoPostal CodigoPostalNavigation { get; set; } = null!;

    [InverseProperty("Casa")]
    public virtual ICollection<Reserva> Reserva { get; set; } = new List<Reserva>();

    [ForeignKey("UtilizadorID")]
    [InverseProperty("Casa")]
    public virtual Utilizador Utilizador { get; set; } = null!;
}

public class CriarCasaDTO
{
	public string Titulo { get; set; } = null!;

	public string? Descricao { get; set; }

	public string Tipo { get; set; } = null!;

	public string Tipologia { get; set; } = null!;

	public decimal Preco { get; set; }

	public string Morada { get; set; } = null!;

    public string CodigoPostal { get; set; } = null!;
}


