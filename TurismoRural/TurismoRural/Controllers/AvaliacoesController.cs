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
