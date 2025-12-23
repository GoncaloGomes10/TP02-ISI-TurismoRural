using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TurismoRural.Context;
using TurismoRural.Models;
using TurismoRural.Services;

namespace TurismoRural.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilizadoresController : ControllerBase
    {
        private readonly TurismoContext _context;
        private readonly JwtService _jwtService;

        public UtilizadoresController(TurismoContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PalavraPass))
                return BadRequest("Email e palavra-passe são obrigatórios.");

            var user = await _context.Utilizador.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized("Email ou palavra-passe inválidos.");

            var passwordHasher = new PasswordHasher<Utilizador>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PalavraPass, request.PalavraPass);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Email ou palavra-passe inválidos.");

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiryTime = refreshToken.Expires;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PalavraPass))
                return BadRequest("Email e palavra-passe são obrigatórios.");

            // Validação de email com domínio completo (exige ponto no domínio)
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(request.Email))
                return BadRequest("Formato de email inválido. Deve incluir domínio (ex: @gmail.com).");

            if (await _context.Utilizador.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email já está registado.");

            var passwordHasher = new PasswordHasher<Utilizador>();
            var newUser = new Utilizador
            {
                Nome = request.Nome,
                Email = request.Email,
                Telemovel = request.Telemovel,
            };

            newUser.PalavraPass = passwordHasher.HashPassword(newUser, request.PalavraPass);

            _context.Utilizador.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Registo feito com sucesso!");
        }

        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var user = await _context.Utilizador.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Refresh token inválido ou expirado.");

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpiryTime = newRefreshToken.Expires;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            });
        }

        [Authorize(Roles = "Support")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            //JWT e o ID do user está no token
            var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var requestingUser = await _context.Utilizador.FindAsync(userIdFromToken);
            if (requestingUser == null || requestingUser.IsSupport != true)
                return Forbid("Apenas administradores podem apagar utilizadores.");

            var userToDelete = await _context.Utilizador.FindAsync(id);
            if (userToDelete == null)
                return NotFound("Utilizador não encontrado.");

            bool temCasas = await _context.Casa.AnyAsync(c => c.UtilizadorID == id);
            if (temCasas)
                return BadRequest("Não é possível apagar utilizadores que tenham casas associadas.");

            _context.Utilizador.Remove(userToDelete);
            await _context.SaveChangesAsync();

            return Ok("Utilizador removido com sucesso.");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Utilizador.FindAsync(userId);

                if (user == null)
                    return NotFound("Utilizador não encontrado.");

                return Ok(new
                {
                    Nome = user.Nome,
                    Email = user.Email,
                    Telemovel = user.Telemovel,
                    IsSupport = user.IsSupport
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Utilizador.FindAsync(userId);
            if (user == null)
                return NotFound("Utilizador não encontrado.");

            bool logoutRequired = false;

            // Atualizar nome e telemóvel
            user.Nome = string.IsNullOrWhiteSpace(request.Nome) ? user.Nome : request.Nome;
            user.Telemovel = string.IsNullOrWhiteSpace(request.Telemovel) ? user.Telemovel : request.Telemovel;

            // Verificar e atualizar email
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(request.Email))
                    return BadRequest("Formato de email inválido.");

                bool emailExists = await _context.Utilizador
                    .AnyAsync(u => u.Email == request.Email && u.UtilizadorID != userId);

                if (emailExists)
                    return BadRequest("Este email já está associado a outra conta.");

                user.Email = request.Email;
                logoutRequired = true;
            }

            // Atualizar palavra-passe
            if (!string.IsNullOrWhiteSpace(request.PalavraPass))
            {
                var passwordHasher = new PasswordHasher<Utilizador>();
                user.PalavraPass = passwordHasher.HashPassword(user, request.PalavraPass);
                logoutRequired = true;
            }

            // Se email ou palavra-passe foi alterado, força logout
            if (logoutRequired)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
            }

            await _context.SaveChangesAsync();

            if (logoutRequired)
                return Ok("Dados atualizados com sucesso. Será necessário iniciar sessão novamente.");

            return Ok("Dados atualizados com sucesso.");
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Utilizador.FindAsync(userId);
            if (user == null)
                return NotFound("Utilizador não encontrado.");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();

            return Ok("Logout efetuado com sucesso.");
        }

        // GET: api/Utilizadores
        [Authorize(Roles = "Support")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            // Garante que quem chama é mesmo suporte
            var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var requestingUser = await _context.Utilizador.FindAsync(userIdFromToken);

            if (requestingUser == null || requestingUser.IsSupport != true)
                return Forbid("Apenas administradores podem listar utilizadores.");

            var utilizadores = await _context.Utilizador
                .Select(u => new UtilizadorListDto
                {
                    UtilizadorID = u.UtilizadorID,
                    Nome = u.Nome,
                    Email = u.Email,
                    Telemovel = u.Telemovel,
                    IsSupport = u.IsSupport ?? false
                })
                .ToListAsync();

            return Ok(utilizadores);
        }




    }
}
