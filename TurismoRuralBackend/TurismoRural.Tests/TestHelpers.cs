using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TurismoRural.Context;

namespace TurismoRural.Tests;

internal static class TestHelpers
{
	/// <summary>
	/// DbContext utilizado exclusivamente para testes unitários.
	/// Impede a execução do método OnConfiguring original do TurismoContext,
	/// evitando a ligação a uma base de dados real (ex.: SQL Server).
	/// </summary>
	private sealed class TestTurismoContext : TurismoContext
	{
		/// <summary>
		/// Inicializa uma nova instância do contexto de testes,
		/// recebendo as opções de configuração do Entity Framework.
		/// </summary>
		/// <param name="options">Opções de configuração do DbContext.</param>
		public TestTurismoContext(DbContextOptions<TurismoContext> options) : base(options) { }

		/// <summary>
		/// Override do método OnConfiguring.
		/// Não chama o método base para garantir que o provider usado
		/// é apenas o InMemory, definido nos testes.
		/// </summary>
		/// <param name="optionsBuilder">Construtor de opções do DbContext.</param>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){}
	}

	/// <summary>
	/// Cria e devolve uma instância do TurismoContext configurada
	/// para usar uma base de dados InMemory.
	/// Cada teste pode usar um nome de base de dados diferente,
	/// garantindo isolamento entre testes.
	/// </summary>
	/// <param name="dbName">Nome da base de dados InMemory.</param>
	/// <returns>Instância do TurismoContext configurada para testes.</returns>
	public static TurismoContext CreateInMemoryDb(string dbName)
	{
		var options = new DbContextOptionsBuilder<TurismoContext>()
			.UseInMemoryDatabase(dbName)
			.EnableSensitiveDataLogging()
			.Options;

		var ctx = new TestTurismoContext(options);
		ctx.Database.EnsureCreated();
		return ctx;
	}

	/// <summary>
	/// Simula um utilizador autenticado num controller.
	/// Injeta um ClaimsPrincipal no HttpContext, permitindo testar
	/// endpoints protegidos por [Authorize] e por roles.
	/// </summary>
	/// <param name="controller">Controller onde o utilizador será injetado.</param>
	/// <param name="userId">Identificador do utilizador.</param>
	/// <param name="role">Role do utilizador (por defeito: "User").</param>
	/// <param name="email">Email do utilizador.</param>
	/// <param name="name">Nome do utilizador.</param>
	public static void SetUser(ControllerBase controller, int userId, string role = "User", string email = "test@test.com", string name = "Teste")
	{
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
			new Claim(ClaimTypes.Role, role),
			new Claim(ClaimTypes.Email, email),
			new Claim(ClaimTypes.Name, name)
		};

		var identity = new ClaimsIdentity(claims, "TestAuth");
		var principal = new ClaimsPrincipal(identity);

		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext { User = principal }
		};
	}
}
