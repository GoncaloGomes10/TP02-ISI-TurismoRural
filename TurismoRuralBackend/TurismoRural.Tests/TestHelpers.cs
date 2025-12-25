using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TurismoRural.Context;

namespace TurismoRural.Tests;

internal static class TestHelpers
{
	// DbContext só para testes (impede UseSqlServer do OnConfiguring do teu TurismoContext)
	private sealed class TestTurismoContext : TurismoContext
	{
		public TestTurismoContext(DbContextOptions<TurismoContext> options) : base(options) { }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// NÃO chamar base.OnConfiguring(optionsBuilder)
			// Assim o provider fica apenas InMemory
		}
	}

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
