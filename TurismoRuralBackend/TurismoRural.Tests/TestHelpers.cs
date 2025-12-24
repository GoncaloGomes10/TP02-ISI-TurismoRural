using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TurismoRural.Context;

namespace TurismoRural.Tests;

internal static class TestHelpers
{
	public static TurismoContext CreateInMemoryDb(string dbName)
	{
		var options = new DbContextOptionsBuilder<TurismoContext>()
			.UseInMemoryDatabase(databaseName: dbName)
			.EnableSensitiveDataLogging()
			.Options;

		return new TurismoContext(options);
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
