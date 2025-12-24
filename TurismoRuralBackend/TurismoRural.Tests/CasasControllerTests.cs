using Microsoft.AspNetCore.Mvc;
using TurismoRural.Controllers;
using TurismoRural.Models;

namespace TurismoRural.Tests;

public class CasasControllerTests
{
	[Fact]
	public async Task PostCasa_DeveRetornarBadRequest_QuandoTipoInvalido()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(PostCasa_DeveRetornarBadRequest_QuandoTipoInvalido));
		var controller = new CasasController(ctx);

		// simula utilizador autenticado porque o controller vai buscar ClaimTypes.NameIdentifier
		TestHelpers.SetUser(controller, userId: 1, role: "User");

		var dto = new CasaDto
		{
			Titulo = "Casa Teste",
			Descricao = "Desc",
			Tipo = "Chale",            // inválido (só aceita Moradia|Apartamento)
			Tipologia = "T1",
			Morada = "Rua X",
			CodigoPostal = "4700-000",
			Localidade = "Braga",
			Distrito = "Braga",
			PrecoNoite = 50
		};

		var result = await controller.PostCasa(dto);
		var bad = Assert.IsType<BadRequestObjectResult>(result);
		Assert.Contains("Tipo invalido", bad.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task PostCasa_DeveRetornarBadRequest_QuandoMoradaJaExiste_MesmoComEspacosVirgulas()
	{
		using var ctx = TestHelpers.CreateInMemoryDb(nameof(PostCasa_DeveRetornarBadRequest_QuandoMoradaJaExiste_MesmoComEspacosVirgulas));
		var controller = new CasasController(ctx);
		TestHelpers.SetUser(controller, userId: 1, role: "User");

		ctx.Casa.Add(new Casa
		{
			Titulo = "Existente",
			Descricao = "x",
			Tipo = "Moradia",
			Tipologia = "T2",
			Morada = "Rua, de Sao  Jeronimo - 10",
			CodigoPostal = "4700-000",
			Localidade = "Braga",
			Distrito = "Braga",
			PrecoNoite = 80,
			UtilizadorID = 1
		});
		await ctx.SaveChangesAsync();

		var dto = new CasaDto
		{
			Titulo = "Nova",
			Descricao = "y",
			Tipo = "Moradia",
			Tipologia = "T2",
			Morada = "Rua de São Jerónimo 10", // deve bater na normalização
			CodigoPostal = "4700-000",
			Localidade = "Braga",
			Distrito = "Braga",
			PrecoNoite = 90
		};

		var result = await controller.PostCasa(dto);
		var bad = Assert.IsType<BadRequestObjectResult>(result);
		Assert.Contains("A casa ja existe", bad.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
	}
}
