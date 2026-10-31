using System.Net;
using System.Net.Http.Json;
using Fcg.Catalog.Application.Games;
using Fcg.Catalog.Application.Games.SearchGames;
using Fcg.Catalog.IntegrationTests._Fixtures;

namespace Fcg.Catalog.IntegrationTests.WebApi.Games;

[Collection(CatalogApiCollection.Name)]
public class GamesEndpointsTests
{
    private readonly CatalogApiFactory _factory;
    private readonly HttpClient _admin;

    public GamesEndpointsTests(CatalogApiFactory factory)
    {
        _factory = factory;
        _admin = factory.CreateAdminClient();
    }

    [Fact]
    public async Task Search_DeveListarJogosDoSeed_QuandoBancoNovo()
    {
        var games = await (await _factory.CreateAnonymousClient().GetAsync("/api/v1/games?pageSize=100")).ReadAsAsync<List<GameCatalogItemResponse>>();

        games.Select(game => game.Title).Should().Contain(["The Witcher 3: Wild Hunt", "Hades", "Stardew Valley", "Celeste"]);
        games.Single(game => game.Title == "Hades").Price.Should().Be(49.90m);
    }

    [Fact]
    public async Task Search_DeveAplicarMaiorDescontoAtivo_QuandoJogoEmPromocao()
    {
        var genre = $"Genero-{Guid.CreateVersion7():N}";
        var promoted = await _admin.CreateGameAsync(79.90m, genre);
        var regular = await _admin.CreateGameAsync(20m, genre);
        await _admin.CreatePromotionAsync(10m, promoted.Id);
        await _admin.CreatePromotionAsync(25m, promoted.Id);

        var response = await _factory.CreateAnonymousClient().GetAsync($"/api/v1/games?genre={genre}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var games = await response.ReadAsAsync<List<GameCatalogItemResponse>>();
        games.Should().HaveCount(2);

        var withPromotion = games.Single(game => game.Id == promoted.Id);
        withPromotion.DiscountPercent.Should().Be(25m);
        withPromotion.EffectivePrice.Should().Be(59.93m);
        withPromotion.ReleaseDate.Should().Be(new DateOnly(2024, 5, 10));

        var withoutPromotion = games.Single(game => game.Id == regular.Id);
        withoutPromotion.DiscountPercent.Should().BeNull();
        withoutPromotion.EffectivePrice.Should().Be(20m);
    }

    [Fact]
    public async Task Search_DeveFiltrarPorPrecoETitulo()
    {
        var genre = $"Genero-{Guid.CreateVersion7():N}";
        var cheap = await _admin.CreateGameAsync(10m, genre, $"Barato {Guid.CreateVersion7():N}");
        await _admin.CreateGameAsync(90m, genre);

        var games = await (await _factory.CreateAnonymousClient()
            .GetAsync($"/api/v1/games?genre={genre}&maxPrice=50&title=barato")).ReadAsAsync<List<GameCatalogItemResponse>>();

        games.Should().ContainSingle().Which.Id.Should().Be(cheap.Id);
    }

    [Fact]
    public async Task Search_NaoDeveListarJogoDesativado()
    {
        var genre = $"Genero-{Guid.CreateVersion7():N}";
        var game = await _admin.CreateGameAsync(30m, genre);

        (await _admin.DeleteAsync($"/api/v1/games/{game.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var games = await (await _factory.CreateAnonymousClient().GetAsync($"/api/v1/games?genre={genre}")).ReadAsAsync<List<GameCatalogItemResponse>>();
        games.Should().BeEmpty();

        var detail = await (await _factory.CreateAnonymousClient().GetAsync($"/api/v1/games/{game.Id}")).ReadAsAsync<GameResponse>();
        detail.Active.Should().BeFalse();
    }

    [Fact]
    public async Task GetById_DeveRetornar404_QuandoJogoInexistente()
    {
        var response = await _factory.CreateAnonymousClient().GetAsync($"/api/v1/games/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_DeveRetornar201ComLocation_QuandoAdministrador()
    {
        var title = $"Novo {Guid.CreateVersion7():N}";

        var response = await _admin.PostAsJsonAsync("/api/v1/games", new { title, description = "d", genre = "RPG", price = 59.90m });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var game = await response.ReadAsAsync<GameResponse>();
        response.Headers.Location!.ToString().Should().Be($"/api/v1/games/{game.Id}");
        game.Title.Should().Be(title);
        game.Active.Should().BeTrue();
    }

    [Fact]
    public async Task Create_DeveRetornar403_QuandoUsuarioComum()
    {
        var response = await _factory.CreateUserClient(Guid.CreateVersion7())
            .PostAsJsonAsync("/api/v1/games", new { title = "X", genre = "RPG", price = 10m });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_DeveRetornar401_QuandoAnonimo()
    {
        var response = await _factory.CreateAnonymousClient().PostAsJsonAsync("/api/v1/games", new { title = "X", genre = "RPG", price = 10m });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_DeveRetornar409_QuandoTituloDuplicado()
    {
        var game = await _admin.CreateGameAsync(10m);

        var response = await _admin.PostAsJsonAsync("/api/v1/games", new { title = game.Title.ToUpperInvariant(), genre = "RPG", price = 10m });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_DeveRetornar400_QuandoDadosInvalidos()
    {
        var response = await _admin.PostAsJsonAsync("/api/v1/games", new { title = "", genre = "RPG", price = -1m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_DeveRetornar200_QuandoJogoExiste()
    {
        var game = await _admin.CreateGameAsync(10m);

        var response = await _admin.PutAsJsonAsync($"/api/v1/games/{game.Id}", new { title = game.Title, description = "nova", genre = "Puzzle", price = 15m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.ReadAsAsync<GameResponse>();
        updated.Genre.Should().Be("Puzzle");
        updated.Price.Should().Be(15m);
    }

    [Fact]
    public async Task UpdateEDelete_DevemRetornar404_QuandoJogoInexistente()
    {
        var id = Guid.CreateVersion7();

        (await _admin.PutAsJsonAsync($"/api/v1/games/{id}", new { title = "X", genre = "Y", price = 1m })).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await _admin.DeleteAsync($"/api/v1/games/{id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
