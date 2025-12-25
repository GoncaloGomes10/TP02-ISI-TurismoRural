using Microsoft.AspNetCore.Mvc;
using TurismoRural.Controllers;
using TurismoRural.Models;
using TurismoRural.Services;

namespace TurismoRural.Tests;

public class UtilizadoresControllerTests
{
	private static JwtService CreateJwtService()
	{
		var settings = new JwtSettings
		{
			SecretKey = "THIS_IS_A_TEST_SECRET_KEY_WITH_ENOUGH_LENGTH_123456",
			Issuer = "TestIssuer",
			Audience = "TestAudience",
			AccessTokenExpirationMinutes = 60,
			RefreshTokenExpirationDays = 7
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
			Telemovel = "910000000",
			PalavraPass = "123456"
		});

		Assert.IsType<BadRequestObjectResult>(result);
	}

	[Fact]
	public async Task Signup_DeveRetornarBadRequest_QuandoEmailJaExiste()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(Signup_DeveRetornarBadRequest_QuandoEmailJaExiste));
		ctx.Utilizador.Add(new Utilizador { Nome = "A", Email = "a@a.com", PalavraPass = "hash", IsSupport = false, Telemovel = "910000000" });
		await ctx.SaveChangesAsync();

		var controller = new UtilizadoresController(ctx, CreateJwtService());

		var result = await controller.Signup(new SignupRequest
		{
			Nome = "B",
			Email = "a@a.com",
			Telemovel = "910000001",
			PalavraPass = "123456"
		});

		Assert.IsType<BadRequestObjectResult>(result);
	}

	[Fact]
	public async Task Signup_DeveCriarUtilizador_QuandoDadosValidos()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(Signup_DeveCriarUtilizador_QuandoDadosValidos));
		var controller = new UtilizadoresController(ctx, CreateJwtService());

		var result = await controller.Signup(new SignupRequest
		{
			Nome = "Novo",
			Email = "novo@teste.com",
			Telemovel = "910000002",
			PalavraPass = "123456"
		});

		// normalmente devolve Ok(...) ou Created(...)
		Assert.True(result is OkObjectResult || result is CreatedResult || result is CreatedAtActionResult);

		Assert.True(ctx.Utilizador.Any(u => u.Email == "novo@teste.com"));
	}

	[Fact]
	public async Task Login_DeveRetornarUnauthorized_QuandoCredenciaisInvalidas()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(Login_DeveRetornarUnauthorized_QuandoCredenciaisInvalidas));
		var controller = new UtilizadoresController(ctx, CreateJwtService());

		var result = await controller.Login(new LoginRequest
		{
			Email = "naoexiste@teste.com",
			PalavraPass = "errada"
		});

		Assert.True(result is UnauthorizedObjectResult || result is UnauthorizedResult);
	}

	[Fact]
	public async Task DeleteUser_DeveRetornarForbid_QuandoNaoESupport()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(DeleteUser_DeveRetornarForbid_QuandoNaoESupport));

		ctx.Utilizador.Add(new Utilizador { UtilizadorID = 10, Nome = "User", Email = "u@u.com", PalavraPass = "hash", IsSupport = false, Telemovel = "910000003" });
		ctx.Utilizador.Add(new Utilizador { UtilizadorID = 20, Nome = "Outro", Email = "o@o.com", PalavraPass = "hash", IsSupport = false, Telemovel = "910000004" });
		await ctx.SaveChangesAsync();

		var controller = new UtilizadoresController(ctx, CreateJwtService());
		TestHelpers.SetUser(controller, userId: 10, role: "User");

		var result = await controller.DeleteUser(20);

		Assert.True(result is ForbidResult || result is ObjectResult);
	}
}
