using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TurismoRural.Models;

public partial class Reserva
{
    [Key]
    public int ReservaID { get; set; }

    public int? CasaID { get; set; }

    public int? UtilizadorID { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly DataFim { get; set; }
    public string Estado { get; set; }

    [ForeignKey("CasaID")]
    [InverseProperty("Reserva")]
    public virtual Casa? Casa { get; set; }

    [ForeignKey("UtilizadorID")]
    [InverseProperty("Reserva")]
    public virtual Utilizador? Utilizador { get; set; }

	public string? GoogleEventId { get; set; }
}

public partial class CriarReservaDTO
{
    public int CasaID { get; set; }

    [Required(ErrorMessage ="É obrigatorio ter data de inicio!")]
    public DateOnly DataInicio { get; set; }

    [Required(ErrorMessage ="É obrigatorio ter data de fim!")]
    public DateOnly DataFim { get; set; }
}

public partial class EditarReservaDTO
{
	[Required(ErrorMessage = "É obrigatorio ter data de inicio!")]
	public DateOnly DataInicio { get; set; }

	[Required(ErrorMessage = "É obrigatorio ter data de fim!")]
	public DateOnly DataFim { get; set; }
}