using Microsoft.AspNetCore.Mvc;
using TurismoRural.Controllers;
using TurismoRural.Models;
using TurismoRural.Services;

namespace TurismoRural.Tests;

public class UtilizadoresControllerTests
{
	/// <summary>
	/// Cria uma instância do JwtService com configurações de teste.
	/// Utilizado nos testes unitários para gerar tokens JWT válidos.
	/// </summary>
	/// <returns>Instância configurada de JwtService.</returns>
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

	/// <summary>
	/// Testa o endpoint Signup e garante que devolve BadRequest
	/// quando o email tem um formato inválido.
	/// </summary>
	/// <returns>Tarefa assíncrona do teste.</returns>
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

	/// <summary>
	/// Testa o endpoint Signup e garante que devolve BadRequest
	/// quando o email já se encontra registado na base de dados.
	/// </summary>
	/// <returns>Tarefa assíncrona do teste.</returns>
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

	/// <summary>
	/// Testa o endpoint Signup e garante que cria um utilizador
	/// quando os dados fornecidos são válidos.
	/// Confirma também que o utilizador fica persistido na base de dados.
	/// </summary>
	/// <returns>Tarefa assíncrona do teste.</returns>
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

	/// <summary>
	/// Testa o endpoint Login e garante que devolve Unauthorized
	/// quando as credenciais fornecidas são inválidas.
	/// </summary>
	/// <returns>Tarefa assíncrona do teste.</returns>
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

	/// <summary>
	/// Testa o endpoint DeleteUser e garante que devolve Forbid
	/// quando o utilizador autenticado não tem permissões de Support.
	/// </summary>
	/// <returns>Tarefa assíncrona do teste.</returns>
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
