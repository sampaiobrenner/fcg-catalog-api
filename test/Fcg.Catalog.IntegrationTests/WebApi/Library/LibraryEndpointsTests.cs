using System.Net;
using Fcg.Catalog.Application.Library.GetMyLibrary;
using Fcg.Catalog.Domain.Library;
using Fcg.Catalog.IntegrationTests._Fixtures;

namespace Fcg.Catalog.IntegrationTests.WebApi.Library;

[Collection(CatalogApiCollection.Name)]
public class LibraryEndpointsTests
{
    private readonly CatalogApiFactory _factory;

    public LibraryEndpointsTests(CatalogApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_DeveRetornarVazio_QuandoUsuarioSemCompras()
    {
        var response = await _factory.CreateUserClient(Guid.CreateVersion7()).GetAsync("/api/v1/library");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.ReadAsAsync<List<LibraryItemResponse>>()).Should().BeEmpty();
    }

    [Fact]
    public async Task Get_DeveRetornarSomenteJogosDoUsuarioDoToken()
    {
        var userId = Guid.CreateVersion7();
        var game = await _factory.CreateAdminClient().CreateGameAsync(25m);
        await _factory.ExecuteDbAsync(async context =>
        {
            context.Add(new LibraryItemPersistenceModel { UserId = userId, GameId = game.Id, PricePaid = 25m, AcquiredAt = DateTime.UtcNow });
            context.Add(new LibraryItemPersistenceModel { UserId = Guid.CreateVersion7(), GameId = game.Id, PricePaid = 25m, AcquiredAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
        });

        var library = await (await _factory.CreateUserClient(userId).GetAsync("/api/v1/library")).ReadAsAsync<List<LibraryItemResponse>>();

        var item = library.Should().ContainSingle().Subject;
        item.GameId.Should().Be(game.Id);
        item.Title.Should().Be(game.Title);
        item.PricePaid.Should().Be(25m);
    }

    [Fact]
    public async Task Get_DeveRetornar401_QuandoAnonimo()
    {
        (await _factory.CreateAnonymousClient().GetAsync("/api/v1/library")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
