using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TurismoRural.Models;

namespace TurismoRural.Services
{
    public class JwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

		/// <summary>
		/// Gera um Access Token (JWT) para um utilizador autenticado.
		/// O token inclui claims de identificação, email, nome e papel (User ou Support).
		/// </summary>
		/// <param name="user">Utilizador autenticado para o qual o token será gerado.</param>
		/// <returns>
		/// Retorna o token JWT em formato de string.
		/// </returns>
		public string GenerateAccessToken(Utilizador user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UtilizadorID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Nome),
                new Claim(ClaimTypes.Role, user.IsSupport == true ? "Support" : "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

		/// <summary>
		/// Gera um Refresh Token seguro.
		/// O token é gerado de forma criptograficamente segura
		/// e tem uma validade definida (7 dias).
		/// </summary>
		/// <returns>
		/// Retorna um objeto RefreshToken contendo o token e a data de expiração.
		/// </returns>
		public RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7)
            };
            
        }
    }

	/// <summary>
	/// Representa um Refresh Token.
	/// Utilizado para renovar o Access Token sem necessidade de novo login.
	/// </summary>
	public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expires {  get; set; }
    }
}
