using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurismoRural.Context;
using TurismoRural.Models;

namespace TurismoRural.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvaliacoesController : ControllerBase
    {
        private readonly TurismoContext _context;

        public AvaliacoesController(TurismoContext context)
        {
            _context = context;
        }

		// POST: api/Avaliacoes/CriarAvaliacao
		/// <summary>
		/// Cria uma nova avaliação para uma casa.
		/// Apenas utilizadores autenticados com o papel "User" podem criar avaliações.
		/// A avaliação só pode ser criada após o término da reserva
		/// e cada utilizador só pode avaliar uma casa uma única vez.
		/// </summary>
		/// <param name="dto">Objeto com os dados da avaliação (CasaID, Nota e Comentário).</param>
		/// <returns>
		/// Retorna OK se a avaliação for criada com sucesso.
		/// Retorna BadRequest se os dados forem inválidos,
		/// se a estadia ainda não tiver terminado
		/// ou se o utilizador já tiver avaliado a casa.
		/// </returns>
		[Authorize(Roles = "User")]
        [HttpPost("CriarAvaliacao")]
        public async Task<IActionResult> CriarAvaliacao([FromBody] CriarAvaliacaoDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Nota < 0 || dto.Nota > 5)
                return BadRequest("A nota tem de ser entre 0 e 5");

            var userId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var hoje = DateOnly.FromDateTime(DateTime.Now);

            bool reservaTerminada = await _context.Reserva.AnyAsync(r =>
                r.CasaID == dto.CasaID && 
                r.UtilizadorID == userId &&
                r.DataFim <= hoje);

            if (!reservaTerminada)
                return BadRequest("So pode avaliar quando a estadia terminar!");

            bool jaAvaliou = await _context.Avaliacao.AnyAsync(a =>
                a.CasaID == dto.CasaID &&
                a.UtilizadorID == userId);

            if (jaAvaliou)
                return BadRequest("Ja avaliou esta casa");
            
			var avalicao = new Avaliacao
            {
                CasaID = dto.CasaID,
                UtilizadorID = userId,
                Nota = dto.Nota,
                Comentario = dto.Comentario
            };

            _context.Avaliacao.Add(avalicao);

            await _context.SaveChangesAsync();

            return Ok("Avaliacao criada com sucesso!");
        }

		// PUT: api/Avalicao/id
		/// <summary>
		/// Edita uma avaliação existente.
		/// Apenas o utilizador que criou a avaliação pode editá-la.
		/// </summary>
		/// <param name="id">Identificador da avaliação a editar.</param>
		/// <param name="dto">Novos dados da avaliação (Nota e Comentário).</param>
		/// <returns>
		/// Retorna OK se a avaliação for editada com sucesso.
		/// Retorna BadRequest se a avaliação não existir ou se a nota for inválida.
		/// Retorna Forbid se o utilizador não for o proprietário da avaliação.
		/// </returns>
		[Authorize(Roles = "User")]
        [HttpPut("EditarAvalicao/{id}")]
        public async Task<IActionResult> EditarAvalicao(int id, [FromBody] EditarAvalicaoDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Nota < 0 || dto.Nota > 5)
                return BadRequest("A nota precisa de estar entre 0 e 5");

			var userId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var avalicao = await _context.Avaliacao.FirstOrDefaultAsync(a => a.AvaliacaoID == id);

            if (avalicao == null)
                return BadRequest("Avaliacao nao encontrada");

            if (avalicao.UtilizadorID != userId)
                return Forbid("Nao tem permissao para editar esta avaliacao");

            avalicao.Nota = dto.Nota;
            avalicao.Comentario = dto.Comentario;

            await _context.SaveChangesAsync();

			return Ok("Avalicao editada com sucesso!");
        }

		// DELETE:api/Avaliacao/id
		/// <summary>
		/// Apaga uma avaliação existente.
		/// Apenas o utilizador que criou a avaliação pode apagá-la.
		/// </summary>
		/// <param name="id">Identificador da avaliação a apagar.</param>
		/// <returns>
		/// Retorna OK se a avaliação for apagada com sucesso.
		/// Retorna NotFound se a avaliação não existir.
		/// Retorna Forbid se o utilizador não for o proprietário da avaliação.
		/// </returns>
		[Authorize(Roles = "User")]
        [HttpDelete("ApagarAvalicao/{id}")]
        public async Task<IActionResult> ApagarAvalicao(int id)
        {

			var userId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var avalicao = await _context.Avaliacao.FirstOrDefaultAsync(a => a.AvaliacaoID == id);

            if (avalicao == null)
                return NotFound("Avaliacao nao encontrada");

            if (avalicao.UtilizadorID != userId)
                return Forbid("Nao tem permissao para apagar esta avaliacao");

            _context.Avaliacao.Remove(avalicao);
            await _context.SaveChangesAsync();

			return Ok("Avalicao apagada com sucesso!");
        }
        
		// GET: api/Avaliacoes/PorCasa/5
		/// <summary>
		/// Obtém todas as avaliações associadas a uma determinada casa.
		/// Inclui o nome do utilizador que realizou cada avaliação.
		/// </summary>
		/// <param name="casaId">Identificador da casa.</param>
		/// <returns>
		/// Retorna uma lista de avaliações ordenadas da mais recente para a mais antiga.
		/// </returns>
		[HttpGet("PorCasa/{casaId}")]
        public async Task<IActionResult> GetAvaliacoesPorCasa(int casaId)
        {
            var avaliacoes = await _context.Avaliacao
                .Include(a => a.Utilizador)
                .Where(a => a.CasaID == casaId)
                .OrderByDescending(a => a.AvaliacaoID)
                .Select(a => new
                {
                    a.AvaliacaoID,
                    a.CasaID,
                    a.UtilizadorID,
                    a.Nota,
                    a.Comentario,
                    NomeUtilizador = a.Utilizador.Nome
                })
                .ToListAsync();

            return Ok(avaliacoes);
        }

    }
}
