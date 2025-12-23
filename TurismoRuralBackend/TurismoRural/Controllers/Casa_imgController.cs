using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurismoRural.Context;
using TurismoRural.Models;

namespace TurismoRural.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Casa_imgController : ControllerBase
    {
        private readonly TurismoContext _context;

        public Casa_imgController(TurismoContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Upload de imagem para uma casa
        /// </summary>
        [HttpPost("upload/{casaId}")]
        public async Task<IActionResult> UploadImagemCasa(IFormFile file, int casaId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nenhum ficheiro enviado.");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "imgs_casa");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var extensao = Path.GetExtension(file.FileName).ToLower();
            var nomeUnico = $"{Guid.NewGuid()}{extensao}";
            var caminhoCompleto = Path.Combine(folderPath, nomeUnico);

            using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var caminhoRelativo = Path.Combine("imgs_casa", nomeUnico).Replace("\\", "/");

            var novaImagem = new Casa_img
            {
                CasaID = casaId,
                PathImagem = caminhoRelativo
            };

            _context.Casa_img.Add(novaImagem);
            await _context.SaveChangesAsync();

            return Ok(new { path = caminhoRelativo });
        }

        /// <summary>
        /// Retorna URLs das imagens associadas à casa
        /// </summary>
        [HttpGet("{casaId}")]
        public async Task<IActionResult> GetImagensDaCasa(int casaId)
        {
            var imagens = await _context.Casa_img
                .Where(i => i.CasaID == casaId)
                .Select(i => new
                {
                    i.ImagemID,
                    i.PathImagem,
                    Url = $"{Request.Scheme}://{Request.Host}/images/casa/{Path.GetFileName(i.PathImagem)}"
                })
                .ToListAsync();

            if (!imagens.Any())
                return NotFound("Nenhuma imagem encontrada para esta casa.");

            return Ok(imagens);
        }

        /// <summary>
        /// Elimina uma imagem (do disco e do banco)
        /// </summary>
        [HttpDelete("{imagemId}")]
        public async Task<IActionResult> DeletarImagem(int imagemId)
        {
            var imagem = await _context.Casa_img.FindAsync(imagemId);
            if (imagem == null)
                return NotFound();

            var caminhoFisico = Path.Combine(Directory.GetCurrentDirectory(), imagem.PathImagem);

            if (System.IO.File.Exists(caminhoFisico))
                System.IO.File.Delete(caminhoFisico);

            _context.Casa_img.Remove(imagem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
