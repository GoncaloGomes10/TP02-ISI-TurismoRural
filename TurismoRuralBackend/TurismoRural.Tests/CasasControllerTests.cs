using Microsoft.AspNetCore.Mvc;
using TurismoRural.Controllers;
using TurismoRural.Models;

namespace TurismoRural.Tests;

public class CasasControllerTests
{

	/// <summary>
	/// Testa o endpoint CriarCasa e garante que devolve BadRequest
	/// quando o DTO contém dados inválidos (ex.: Tipo inválido).
	/// </summary>
	/// <returns>Tarefa assíncrona do teste.</returns>
	[Fact]
	public async Task CriarCasa_DeveRetornarBadRequest_QuandoDadosInvalidos()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(CriarCasa_DeveRetornarBadRequest_QuandoDadosInvalidos));
		var controller = new CasasController(ctx);

		TestHelpers.SetUser(controller, userId: 1, role: "Support");

		var dto = new CriarCasaDTO
		{
			Titulo = "Sem regras",
			Descricao = "Desc",
			Tipo = "Chale",      // invalido -> BadRequest antes de gravar
			Tipologia = "T1",
			Preco = 50,
			Morada = "Rua X",
			CodigoPostal = "4700-000"
		};

		var result = await controller.CriarCasa(dto);

		Assert.IsType<BadRequestObjectResult>(result);
	}

	/// <summary>
	/// Testa o endpoint CriarCasa e garante que devolve BadRequest
	/// quando o campo Tipo é inválido (não é "Moradia" nem "Apartamento").
	/// </summary>
	/// <returns>Tarefa assíncrona do teste.</returns>
	[Fact]
	public async Task CriarCasa_DeveRetornarBadRequest_QuandoTipoInvalido()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(CriarCasa_DeveRetornarBadRequest_QuandoTipoInvalido));
		var controller = new CasasController(ctx);

		TestHelpers.SetUser(controller, userId: 1, role: "Support");

		var dto = new CriarCasaDTO
		{
			Titulo = "Casa Teste",
			Descricao = "Desc",
			Tipo = "Chale",
			Tipologia = "T1",
			Preco = 50,
			Morada = "Rua X",
			CodigoPostal = "4700-000"
		};

		var result = await controller.CriarCasa(dto);

		Assert.IsType<BadRequestObjectResult>(result);
	}

	/// <summary>
	/// Testa o endpoint CriarCasa e garante que devolve BadRequest
	/// quando já existe uma casa com a mesma morada (evita duplicados).
	/// </summary>
	/// <returns>Tarefa assíncrona do teste.</returns>
	[Fact]
	public async Task CriarCasa_DeveRetornarBadRequest_QuandoMoradaJaExiste()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(CriarCasa_DeveRetornarBadRequest_QuandoMoradaJaExiste));
		var controller = new CasasController(ctx);

		TestHelpers.SetUser(controller, userId: 1, role: "Support");

		ctx.Casa.Add(new Casa
		{
			Titulo = "Existente",
			Descricao = "x",
			Tipo = "Moradia",
			Tipologia = "T2",
			Preco = 80,
			Morada = "Rua de Sao Jeronimo 10",
			CodigoPostal = "4700-000"
		});
		await ctx.SaveChangesAsync();

		var dto = new CriarCasaDTO
		{
			Titulo = "Nova",
			Descricao = "y",
			Tipo = "Moradia",
			Tipologia = "T2",
			Preco = 90,
			Morada = "Rua de Sao Jeronimo 10", // IGUAL
			CodigoPostal = "4700-000"
		};

		var result = await controller.CriarCasa(dto);

		Assert.IsType<BadRequestObjectResult>(result);
	}

	/// <summary>
	/// Testa o endpoint CriarCasa e garante que cria uma casa
	/// quando os dados são válidos.
	/// Confirma também que a casa ficou persistida na BD em memória.
	/// </summary>
	[Fact]
	public async Task CriarCasa_DeveCriarCasa_QuandoDadosValidos()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(CriarCasa_DeveCriarCasa_QuandoDadosValidos));
		var controller = new CasasController(ctx);

		TestHelpers.SetUser(controller, userId: 1, role: "Support");

		var dto = new CriarCasaDTO
		{
			Titulo = "Casa OK",
			Descricao = "Desc",
			Tipo = "Moradia",
			Tipologia = "T3",
			Preco = 120,
			Morada = "Rua Nova 1",
			CodigoPostal = "4700-001"
		};

		var result = await controller.CriarCasa(dto);

		Assert.True(result is OkObjectResult || result is CreatedResult || result is CreatedAtActionResult);
		Assert.True(ctx.Casa.Any(c => c.Morada == "Rua Nova 1"));
	}
}
