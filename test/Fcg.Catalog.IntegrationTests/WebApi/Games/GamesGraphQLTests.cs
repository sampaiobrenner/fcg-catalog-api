using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Fcg.Catalog.IntegrationTests._Fixtures;

namespace Fcg.Catalog.IntegrationTests.WebApi.Games;

[Collection(CatalogApiCollection.Name)]
public class GamesGraphQLTests
{
    private readonly CatalogApiFactory _factory;

    public GamesGraphQLTests(CatalogApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Games_DeveFiltrarPorGenero_QuandoConsultaComWhere()
    {
        var response = await _factory.CreateAnonymousClient().PostAsJsonAsync("/graphql", new
        {
            query = """{ games(where: { genre: { eq: "RPG" } }, order: { title: ASC }) { title price } }"""
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.TryGetProperty("errors", out var errors).Should().BeFalse(errors.ToString());

        var games = document.RootElement.GetProperty("data").GetProperty("games").EnumerateArray().ToList();
        games.Should().NotBeEmpty();
        games.Select(game => game.GetProperty("title").GetString()).Should().Contain("The Witcher 3: Wild Hunt");
        games.Should().OnlyContain(game => game.GetProperty("price").GetDecimal() > 0);
    }
}
