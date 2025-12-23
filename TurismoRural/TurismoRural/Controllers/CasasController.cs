using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurismoRural.Context;
using TurismoRural.Models;
using System.Text.RegularExpressions;

namespace TurismoRural.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CasasController : ControllerBase
    {
        private readonly TurismoContext _context;

        public CasasController(TurismoContext context)
        {
            _context = context;
        }

        // POST: api/Casas/CriarCasa
        [Authorize(Roles = "Support")]
        [HttpPost("CriarCasa")]
        public async Task<IActionResult> CriarCasa([FromBody] CriarCasaDTO dto)
        {
            //Verifica se está a seguir os dados do dto
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Preco <= 0)
                return BadRequest("O preço deve ser superior a 0!");

            //Regex para tipologia
            if (!Regex.IsMatch(dto.Tipologia, @"^T[1-4]$", RegexOptions.IgnoreCase))
                return BadRequest("Tipologia invalida!");

            //Regex para tipo
            if (!Regex.IsMatch(dto.Tipo, @"^(Moradia|Apartamento)$", RegexOptions.IgnoreCase))
                return BadRequest("Tipo invalido!");

            //Colocar a primeira letra em maiusculo e o restante em minusculo
			dto.Tipo = char.ToUpper(dto.Tipo[0]) + dto.Tipo.Substring(1).ToLower();

            //Remove todos os espacos em braco, virgulas, pontos e hifens
            string moradaNormalizada = Regex.Replace(dto.Morada.Trim().ToLower(),@"[\s,.-]","");

            //Mete todas as casas da BD para a memoria
            var moradas = await _context.Casa.Select(c => c.Morada).ToListAsync();

            //Verifica se ja existe alguma casa com a morada igual
            bool moradaJaExiste = moradas.Any(m => Regex.Replace(m.Trim().ToLower(),@"[\s,.-]","") == moradaNormalizada);

            if (moradaJaExiste)
                return BadRequest("A casa ja existe");

            //Associa a casa ao utilizador autenticado
            var userId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);


            var casa = new Casa
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Tipo = dto.Tipo,
                Tipologia = dto.Tipologia.ToUpper(),
                Preco = dto.Preco,
                Morada = dto.Morada,
                CodigoPostal = dto.CodigoPostal,
                UtilizadorID = userId
            };

            _context.Casa.Add(casa);
            await _context.SaveChangesAsync();

            return Ok("Criada com sucesso");
        }

        // PUT: api/Casas/id
        [Authorize(Roles = "Support")]
        [HttpPut("EditarCasa/{id}")]
        public async Task<IActionResult> EditarCasa(int id, [FromBody] CriarCasaDTO dto)
        {
            if(!ModelState.IsValid) 
                return BadRequest(ModelState);

			if (dto.Preco <= 0)
				return BadRequest("O preço deve ser superior a 0!");

			//Regex para tipologia
			if (!Regex.IsMatch(dto.Tipologia, @"^T[1-4]$", RegexOptions.IgnoreCase))
				return BadRequest("Tipologia invalida!");

			//Regex para tipo
			if (!Regex.IsMatch(dto.Tipo, @"^(Moradia|Apartamento)$", RegexOptions.IgnoreCase))
				return BadRequest("Tipo invalido!");

            //Procura a casa na BD
            var casa = await _context.Casa.FindAsync(id);
            if(casa == null)
                return NotFound("Casa nao encontrada");

            //Coloca a primeira letra em maiusculo e as restantes em minusculo
			string tipoNormalizado = char.ToUpper(dto.Tipo[0]) + dto.Tipo.Substring(1).ToLower();

            //Mete sem em maiusculo ex:T1
			string tipologiaNormalizada = dto.Tipologia.ToUpper();

            //Remove os espacos, virgulas, pontos e hifens
			string moradaNormalizada = Regex.Replace(dto.Morada.Trim().ToLower(), @"[\s,.-]", "");

            //Verifico outras casas sem contar com a que estou a editar
            var outrasMoradas = await _context.Casa.Where(c=> c.CasaID != id).Select(c=> c.Morada).ToListAsync();

            //Verifico se a morada que estou a editar ja existe
            bool moradaJaExiste = outrasMoradas.Any(m=> Regex.Replace(m.Trim().ToLower(), @"[\s,.-]", "") == moradaNormalizada);

			if (moradaJaExiste)
				return BadRequest("Já existe outra casa com essa morada.");

			casa.Titulo = dto.Titulo;
			casa.Descricao = dto.Descricao;
			casa.Tipo = tipoNormalizado;
			casa.Tipologia = tipologiaNormalizada;
			casa.Preco = dto.Preco;
			casa.Morada = dto.Morada;
			casa.CodigoPostal = dto.CodigoPostal;

			await _context.SaveChangesAsync();

			return Ok("Casa editada com sucesso");
		}

        // DELETE: api/Casas/id
        [Authorize(Roles = "Support")]
        [HttpDelete("DeleteCasa/{id}")]
        public async Task<IActionResult> DeleteCasa(int id)
        {
            var casa = await _context.Casa.FindAsync(id);

            if (casa == null)
                return NotFound("Casa nao encontrada");

            _context.Casa.Remove(casa);
            await _context.SaveChangesAsync();

            return Ok("Casas apagada com sucesso");
        }

        // GET: api/Casas
        [HttpGet("Casas")]
        public async Task<IActionResult> GetCasas()
        {
            var casas = await _context.Casa
                .Select(c => new
                {
                    c.CasaID,
                    c.Titulo,
                    c.Descricao,
                    c.Tipo,
                    c.Tipologia,
                    c.Preco,
                    c.Morada,
                    c.CodigoPostal
                })
                .ToListAsync();

            return Ok(casas);
        }

        // GET: api/Casas/id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCasa(int id)
        {
            var casa = await _context.Casa
                .Where(c => c.CasaID == id)
                .Select(c => new
                {
                    c.CasaID,
                    c.Titulo,
                    c.Descricao,
                    c.Tipo,
                    c.Tipologia,
                    c.Preco,
                    c.Morada,
                    c.CodigoPostal
                })
                .FirstOrDefaultAsync();

            if (casa == null)
                return NotFound("Casa não encontrada.");

            return Ok(casa);
        }

    }
}
