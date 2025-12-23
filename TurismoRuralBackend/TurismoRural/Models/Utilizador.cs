using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TurismoRural.Models;

public partial class Utilizador
{
    [Key]
    public int UtilizadorID { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Nome { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string Telemovel { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string PalavraPass { get; set; } = null!;

    public bool? IsSupport { get; set; }
    
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    [InverseProperty("Utilizador")]
    public virtual ICollection<Avaliacao> Avaliacao { get; set; } = new List<Avaliacao>();

    [InverseProperty("Utilizador")]
    public virtual ICollection<Casa> Casa { get; set; } = new List<Casa>();

    [InverseProperty("Utilizador")]
    public virtual ICollection<Reserva> Reserva { get; set; } = new List<Reserva>();
}

public class LoginRequest
{
    public string Email { get; set; }
    public string PalavraPass { get; set; }
}

public class SignupRequest
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Telemovel { get; set; }
    public string PalavraPass { get; set; }
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Telemovel { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PalavraPass { get; set; } = string.Empty;
}

public class UtilizadorListDto
{
    public int UtilizadorID { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telemovel { get; set; }
    public bool IsSupport { get; set; }
}
