using System.Net;
using System.Net.Http.Json;
using Fcg.Catalog.Application.Promotions;
using Fcg.Catalog.IntegrationTests._Fixtures;

namespace Fcg.Catalog.IntegrationTests.WebApi.Promotions;

[Collection(CatalogApiCollection.Name)]
public class PromotionsEndpointsTests
{
    private readonly CatalogApiFactory _factory;
    private readonly HttpClient _admin;

    public PromotionsEndpointsTests(CatalogApiFactory factory)
    {
        _factory = factory;
        _admin = factory.CreateAdminClient();
    }

    [Fact]
    public async Task CreateListDelete_DeveGerenciarPromocao_QuandoAdministrador()
    {
        var game = await _admin.CreateGameAsync(40m);
        var promotion = await _admin.CreatePromotionAsync(15m, game.Id);

        var promotions = await (await _admin.GetAsync("/api/v1/promotions")).ReadAsAsync<List<PromotionResponse>>();
        var listed = promotions.Single(item => item.Id == promotion.Id);
        listed.GameIds.Should().Equal(game.Id);
        listed.Active.Should().BeTrue();

        (await _admin.DeleteAsync($"/api/v1/promotions/{promotion.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        promotions = await (await _admin.GetAsync("/api/v1/promotions")).ReadAsAsync<List<PromotionResponse>>();
        promotions.Single(item => item.Id == promotion.Id).Active.Should().BeFalse();
    }

    [Fact]
    public async Task Create_DeveRetornar404_QuandoJogoInexistente()
    {
        var response = await _admin.PostAsJsonAsync("/api/v1/promotions", new
        {
            name = "Fantasma",
            discountPercent = 10m,
            startsAt = DateTime.UtcNow,
            endsAt = DateTime.UtcNow.AddDays(1),
            gameIds = new[] { Guid.CreateVersion7() }
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_DeveRetornar400_QuandoDatasInvertidas()
    {
        var response = await _admin.PostAsJsonAsync("/api/v1/promotions", new
        {
            name = "Invertida",
            discountPercent = 10m,
            startsAt = DateTime.UtcNow,
            endsAt = DateTime.UtcNow.AddDays(-1),
            gameIds = new[] { Guid.CreateVersion7() }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_DeveRetornar404_QuandoPromocaoInexistente()
    {
        (await _admin.DeleteAsync($"/api/v1/promotions/{Guid.CreateVersion7()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_DeveRetornar403_QuandoUsuarioComum()
    {
        (await _factory.CreateUserClient(Guid.CreateVersion7()).GetAsync("/api/v1/promotions")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
