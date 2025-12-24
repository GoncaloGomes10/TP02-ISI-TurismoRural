using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurismoRural.Context;
using TurismoRural.Models;
using TurismoRural.Services;

namespace TurismoRural.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly TurismoContext _context;
        private readonly GoogleCalendarService _googleCalendar;

        public ReservasController(TurismoContext context, GoogleCalendarService googleCalendar)
        {
            _context = context;
            _googleCalendar = googleCalendar;
        }

        // POST: api/Reservas/CriarReserva
        [Authorize(Roles="User")]
        [HttpPost("CriarReserva")]
        public async Task<IActionResult> CriarReserva([FromBody] CriarReservaDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.DataInicio < DateOnly.FromDateTime(DateTime.UtcNow) || dto.DataFim <= DateOnly.FromDateTime(DateTime.Now))
                return BadRequest("A data de inicio nao pode ser no passado e a data de fim nao pode ser anterior a data de inicio");


            var userId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var casaExiste = await _context.Casa.AnyAsync(c => c.CasaID == dto.CasaID);

            if (!casaExiste)
                return NotFound("Casa nao existe!");

            bool conflito = await _context.Reserva.AnyAsync(r =>
                r.CasaID == dto.CasaID &&
                r.Estado != "Cancelado" &&
                (
                    (dto.DataInicio >= r.DataInicio && dto.DataInicio < r.DataFim) || //Inicio durante outra reserva
                    (dto.DataFim > r.DataInicio && dto.DataFim <= r.DataFim) || //Fim durante outra reserva
                    (dto.DataInicio <= r.DataInicio && dto.DataFim >= r.DataFim) //Intervalo engloba outra reserva
                )
            );

            if (conflito)
                return BadRequest("A casa ja esta reservada nessa data!");

            var reserva = new Reserva
            {
                CasaID = dto.CasaID,
                UtilizadorID = userId,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                Estado = "Pendente"
            };

            _context.Reserva.Add( reserva );
            await _context.SaveChangesAsync();

			var summary = $"Reserva Casa {dto.CasaID} (User {userId})";
			var desc = $"ReservaID: {reserva.ReservaID}\nCasaID: {reserva.CasaID}\nUtilizadorID: {userId}\nEstado: {reserva.Estado}";

			// converter DateOnly → DateTime
			var start = reserva.DataInicio
				.ToDateTime(new TimeOnly(14, 0))
				.ToUniversalTime();

			var end = reserva.DataFim
				.ToDateTime(new TimeOnly(11, 0))
				.ToUniversalTime();

			var eventId = await _googleCalendar.CreateEventAsync(
				summary,
				start,
				end,
				desc
			);

			Console.WriteLine("GOOGLE EVENT ID: " + eventId);

			// guardar o ID do evento
			reserva.GoogleEventId = eventId;
			await _context.SaveChangesAsync();

			return Ok("Reserva criada com sucesso!");
		}


		// PUT: api/Reservas/id
		[Authorize(Roles = "User")]
        [HttpPut("EditarReserva/{id}")]
        public async Task<IActionResult> EditarReserva(int id, [FromBody] EditarReservaDTO dto)
        {
            if(!ModelState.IsValid) 
                return BadRequest(ModelState);

			if (dto.DataInicio < DateOnly.FromDateTime(DateTime.Now) || dto.DataFim <= dto.DataInicio)
				return BadRequest("Datas inválidas.");


			var userId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            
            var reserva = await _context.Reserva.FindAsync(id);

            if (reserva == null)
                return NotFound("Nao foi encontrada a reserva!");

            if (reserva.UtilizadorID != userId)
                return Forbid("Nao tem permissao para editar esta reserva");

            if (reserva.Estado != "Pendente")
                return BadRequest("So pode editar reservas cujo estado seja Pendente");

			bool conflito = await _context.Reserva.AnyAsync(r =>
                r.ReservaID != id &&
				r.CasaID == reserva.CasaID &&
				r.Estado != "Cancelado" &&
				(
					(dto.DataInicio >= r.DataInicio && dto.DataInicio < r.DataFim) || //Inicio durante outra reserva
					(dto.DataFim > r.DataInicio && dto.DataFim <= r.DataFim) || //Fim durante outra reserva
					(dto.DataInicio <= r.DataInicio && dto.DataFim >= r.DataFim) //Intervalo engloba outra reserva
				)
			);

			if (conflito)
				return BadRequest("A casa ja esta reservada nessa data!");

            reserva.DataInicio = dto.DataInicio;
            reserva.DataFim = dto.DataFim;

            await _context.SaveChangesAsync();

			if (!string.IsNullOrWhiteSpace(reserva.GoogleEventId))
			{
				// Ajusta isto ao teu “significado” de data fim (se for checkout, normalmente é dia seguinte 00:00)
				var startUtc = dto.DataInicio.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
				var endUtc = dto.DataFim.ToDateTime(TimeOnly.MinValue).ToUniversalTime();

				var summary = $"Reserva #{reserva.ReservaID}";
				var description = $"CasaID: {reserva.CasaID} | UtilizadorID: {reserva.UtilizadorID}";

				try
				{
					await _googleCalendar.UpdateEventAsync(reserva.GoogleEventId, summary, description, startUtc, endUtc);
				}
				catch
				{
					// Não rebento o endpoint porque a BD já ficou consistente.
					// Se quiseres, podes devolver 200 com aviso.
					return Ok("Reserva editada com sucesso! (Aviso: não foi possível atualizar o Google Calendar)");
				}
			}

			return Ok("Reserva editada com sucesso!");
        }

        // DELETE :api/Reserva/id
        [Authorize(Roles = "User")]
        [HttpDelete("CancelarReserva/{id}")]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            
            var userId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var reserva = await _context.Reserva.FindAsync(id);

            if (reserva == null)
                return NotFound("Reserva nao foi encontrada!");

            if (reserva.UtilizadorID != userId)
                return Forbid("Nao tem permissao para cancelar esta reserva!");

            DateOnly hoje = DateOnly.FromDateTime(DateTime.Now);
            if (reserva.DataInicio <= hoje)
                return BadRequest("Nao pode cancelar uma reserva que ja comecou!");

            reserva.Estado = "Cancelada";
            await _context.SaveChangesAsync();

			if (!string.IsNullOrWhiteSpace(reserva.GoogleEventId))
			{
				try
				{
					await _googleCalendar.DeleteEventAsync(reserva.GoogleEventId);

					// opcional: limpar o id depois de apagar
					reserva.GoogleEventId = null;
					await _context.SaveChangesAsync();
				}
				catch
				{
					return Ok("Reserva cancelada com sucesso! (Aviso: não foi possível remover do Google Calendar)");
				}
			}

			return Ok("Reserva cancelada com sucesso!");
        }

        // GET: api/Reservas/PorCasa/id
        [HttpGet("PorCasa/{casaId}")]
        public async Task<IActionResult> GetReservasPorCasa(int casaId)
        {
            var reservas = await _context.Reserva
                .Where(r => r.CasaID == casaId)
                .OrderBy(r => r.DataInicio)
                .Select(r => new
                {
                    r.ReservaID,
                    r.CasaID,
                    r.DataInicio,
                    r.DataFim,
                    r.Estado
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // GET: api/Reservas/MinhasReservas
        [HttpGet("MinhasReservas")]
        [Authorize]
        public async Task<IActionResult> GetMinhasReservas()
        {
            var userId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var reservas = await _context.Reserva
                .Include(r => r.Casa)
                .Where(r => r.UtilizadorID == userId)
                .OrderByDescending(r => r.DataInicio)
                .Select(r => new
                {
                    r.ReservaID,
                    r.DataInicio,
                    r.DataFim,
                    r.Estado,
                    r.CasaID,
                    CasaTitulo = r.Casa.Titulo,
                    CasaMorada = r.Casa.Morada,
                    TemAvaliacao = _context.Avaliacao
                    .Any(a => a.CasaID == r.CasaID && a.UtilizadorID == userId),

                    AvaliacaoID = _context.Avaliacao
                    .Where(a => a.CasaID == r.CasaID && a.UtilizadorID == userId)
                    .Select(a => a.AvaliacaoID)
                    .FirstOrDefault(),

                    AvaliacaoNota = _context.Avaliacao
                    .Where(a => a.CasaID == r.CasaID && a.UtilizadorID == userId)
                    .Select(a => a.Nota)
                    .FirstOrDefault(),

                    AvaliacaoComentario = _context.Avaliacao
                    .Where(a => a.CasaID == r.CasaID && a.UtilizadorID == userId)
                    .Select(a => a.Comentario)
                    .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // PUT: api/Reservas/AtualizarEstados
        [HttpPut("AtualizarEstados")]
        public async Task<IActionResult> AtualizarEstados()
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);

            var reservasParaTerminar = await _context.Reserva
            .Where(r => r.Estado == "Pendente" && r.DataFim <= hoje)
            .ToListAsync();


            foreach (var r in reservasParaTerminar)
            {
                r.Estado = "Terminada";
            }

            await _context.SaveChangesAsync();

            return Ok($"{reservasParaTerminar.Count} reservas atualizadas para Terminada.");
        }
    }
}
