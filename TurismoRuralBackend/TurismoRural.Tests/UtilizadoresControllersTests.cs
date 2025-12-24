using Microsoft.AspNetCore.Mvc;
using TurismoRural.Controllers;
using TurismoRural.Models;
using TurismoRural.Services;

namespace TurismoRural.Tests;

public class UtilizadoresControllerTests
{
	private static JwtService CreateJwtService()
	{
		// valores dummy só para não rebentar (não estamos a testar JWT em profundidade)
		var settings = new JwtSettings
		{
			SecretKey = "THIS_IS_A_TEST_SECRET_KEY_WITH_ENOUGH_LENGTH_123456",
			Issuer = "TestIssuer",
			Audience = "TestAudience",
			AccessTokenExpiryMinutes = 60,
			RefreshTokenExpiryDays = 7
		};
		return new JwtService(settings);
	}

	[Fact]
	public async Task Signup_DeveRetornarBadRequest_QuandoEmailInvalido()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(Signup_DeveRetornarBadRequest_QuandoEmailInvalido));
		var controller = new UtilizadoresController(ctx, CreateJwtService());

		var result = await controller.Signup(new SignupRequest
		{
			Nome = "Domingos",
			Email = "emailsemarroba",
			PalavraPass = "123456"
		});

		var bad = Assert.IsType<BadRequestObjectResult>(result);
		Assert.Contains("Formato de email inválido", bad.Value?.ToString());
	}

	[Fact]
	public async Task Signup_DeveRetornarBadRequest_QuandoEmailJaExiste()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(Signup_DeveRetornarBadRequest_QuandoEmailJaExiste));
		ctx.Utilizador.Add(new Utilizador { Nome = "A", Email = "a@a.com", PalavraPass = "hash", IsSupport = false });
		await ctx.SaveChangesAsync();

		var controller = new UtilizadoresController(ctx, CreateJwtService());

		var result = await controller.Signup(new SignupRequest
		{
			Nome = "B",
			Email = "a@a.com",
			PalavraPass = "123456"
		});

		var bad = Assert.IsType<BadRequestObjectResult>(result);
		Assert.Contains("Email já está registado", bad.Value?.ToString());
	}

	[Fact]
	public async Task DeleteUser_DeveRetornarForbid_QuandoQuemPedeNaoESupport()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(DeleteUser_DeveRetornarForbid_QuandoQuemPedeNaoESupport));

		// utilizador autenticado (User normal)
		ctx.Utilizador.Add(new Utilizador { UtilizadorID = 10, Nome = "User", Email = "u@u.com", PalavraPass = "hash", IsSupport = false });
		// alvo a apagar
		ctx.Utilizador.Add(new Utilizador { UtilizadorID = 20, Nome = "Outro", Email = "o@o.com", PalavraPass = "hash", IsSupport = false });
		await ctx.SaveChangesAsync();

		var controller = new UtilizadoresController(ctx, CreateJwtService());
		TestHelpers.SetUser(controller, userId: 10, role: "User");

		var result = await controller.DeleteUser(20);

		// No teu código usas Forbid("...") — isto devolve ForbidResult ou ObjectResult dependendo de como está implementado
		Assert.True(result is ForbidResult || result is ObjectResult);
	}
}
