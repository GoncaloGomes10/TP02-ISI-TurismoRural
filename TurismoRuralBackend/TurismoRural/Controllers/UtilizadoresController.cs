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

		/// <summary>
		/// Autentica um utilizador (login).
		/// Valida as credenciais (email e palavra-passe), gera um AccessToken (JWT) e um RefreshToken.
		/// O RefreshToken é guardado na base de dados com a respetiva data de expiração.
		/// </summary>
		/// <param name="request">Dados de login (Email e PalavraPass).</param>
		/// <returns>
		/// Retorna OK com AccessToken e RefreshToken se as credenciais forem válidas.
		/// Retorna BadRequest se email/palavra-passe não forem enviados.
		/// Retorna Unauthorized se as credenciais forem inválidas.
		/// </returns>
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

		/// <summary>
		/// Regista um novo utilizador (signup).
		/// Valida email, valida palavra-passe e impede registos duplicados pelo mesmo email.
		/// A palavra-passe é armazenada na base de dados em formato "hash".
		/// </summary>
		/// <param name="request">Dados do registo (Nome, Email, Telemovel e PalavraPass).</param>
		/// <returns>
		/// Retorna OK se o utilizador for criado com sucesso.
		/// Retorna BadRequest se os dados forem inválidos ou se o email já existir.
		/// </returns>
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

		/// <summary>
		/// Renova os tokens de autenticação (refresh).
		/// Recebe um RefreshToken válido, verifica se existe e se não expirou,
		/// e devolve um novo AccessToken e um novo RefreshToken.
		/// </summary>
		/// <param name="request">Contém o RefreshToken atual.</param>
		/// <returns>
		/// Retorna OK com novos tokens se o RefreshToken for válido.
		/// Retorna Unauthorized se o RefreshToken não existir ou estiver expirado.
		/// </returns>
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

		/// <summary>
		/// Elimina um utilizador pelo seu ID.
		/// Apenas utilizadores com papel "Support" podem executar esta ação.
		/// Não permite apagar utilizadores que tenham casas associadas.
		/// </summary>
		/// <param name="id">Identificador do utilizador a apagar.</param>
		/// <returns>
		/// Retorna OK se o utilizador for removido com sucesso.
		/// Retorna Forbid se quem pede não for administrador/support.
		/// Retorna NotFound se o utilizador não existir.
		/// Retorna BadRequest se o utilizador tiver casas associadas.
		/// </returns>
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

		/// <summary>
		/// Obtém o perfil do utilizador autenticado.
		/// Devolve dados básicos: nome, email, telemóvel e se é suporte.
		/// </summary>
		/// <returns>
		/// Retorna OK com os dados do utilizador.
		/// Retorna NotFound se o utilizador não existir.
		/// Retorna StatusCode 500 em caso de erro inesperado.
		/// </returns>
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

		/// <summary>
		/// Atualiza os dados do utilizador autenticado.
		/// Permite atualizar nome, telemóvel, email e palavra-passe.
		/// Se o email ou a palavra-passe forem alterados, invalida o RefreshToken (força novo login).
		/// </summary>
		/// <param name="request">Dados a atualizar (Nome, Telemovel, Email e/ou PalavraPass).</param>
		/// <returns>
		/// Retorna OK se a atualização for bem-sucedida.
		/// Retorna BadRequest se o email tiver formato inválido ou já estiver em uso.
		/// Retorna NotFound se o utilizador não existir.
		/// </returns>
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

		/// <summary>
		/// Efetua logout do utilizador autenticado.
		/// Invalida o RefreshToken, obrigando novo login para voltar a obter tokens.
		/// </summary>
		/// <returns>
		/// Retorna OK se o logout for efetuado com sucesso.
		/// Retorna NotFound se o utilizador não existir.
		/// </returns>
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
		/// <summary>
		/// Lista todos os utilizadores.
		/// Apenas utilizadores com papel "Support" podem executar esta ação.
		/// </summary>
		/// <returns>
		/// Retorna OK com a lista de utilizadores.
		/// Retorna Forbid se quem pede não for administrador/support.
		/// </returns>
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
