using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TurismoRural.Context;
using TurismoRural.Models;

namespace TurismoRural.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodigoPostalController : ControllerBase
    {
        private readonly TurismoContext _context;

        public CodigoPostalController(TurismoContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Verificação se Código Postal existe
        /// </summary>
        /// <param name="codigoPostal"> Código Postal </param>
        /// <returns>
        ///     Retorna resultado da query
        /// </returns>
        private Task<bool> CodigoPostalExists(string codigoPostal)
        {
            return _context.CodigoPostal.AnyAsync(e => e.CodigoPostal1 == codigoPostal);
        }

        /// <summary>
        /// Validar Código Postal
        /// </summary>
        /// <param name="codigoPostal"> Código Postal</param>
        /// <returns>
        ///     Retorna BadRequest se não receber nenhum Código Postal
        ///     Retorna NotFound se o Código Postal não for válido
        ///     Retorna Ok com o mesmo Código Postal recebido
        /// </returns>
        [HttpGet("validate={codigoPostal}")]
        public async Task<ActionResult<CodigoPostal>> GetCodigoPostal(string codigoPostal)
        {
            if (string.IsNullOrEmpty(codigoPostal))
            {
                return BadRequest();
            }
            if (!await CodigoPostalExists(codigoPostal))
            {
                return NotFound();
            }
            return Ok(codigoPostal);
        }

        /// <summary>
        /// Receber a morada inteira
        /// </summary>
        /// <param name="codigoPostal"> Código Postal </param>
        /// <returns>
        ///     Retorna NotFound se não encontrar algum dos campos (Código Postal, Localidade, Distrito)
        ///     Retorna Ok com a localidade e distrito do Código Postal recebido
        /// </returns>
        [HttpGet("{codigoPostal}")]
        public async Task<ActionResult<CodigoPostal>> GetFullAddress(string codigoPostal)
        {
            // Busca o ID da Localidade correspondente ao Código Postal
            var localID = await _context.CodigoPostal
                .Where(l => l.CodigoPostal1 == codigoPostal)
                .Select(l => l.LocalidadeID)
                .FirstOrDefaultAsync();

            // Se não encontrou, retorna NotFound
            if (localID != 0)
            {
                var localidade = await _context.Localidade
                    .Where(l => l.LocalidadeID == localID)
                    .Select(l => new { l.DistritoID, l.Nome })
                    .FirstOrDefaultAsync();

                if (localidade != null)
                {
                    var distrito = await _context.Distrito
                        .Where(d => d.DistritoID == localidade.DistritoID)
                        .Select(d => d.Nome)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrWhiteSpace(distrito))
                    {
                        return Ok(new
                        {
                            localidade = localidade.Nome,
                            distrito
                        });
                    }
                }
            }

            return NotFound();
        }

    }
}
