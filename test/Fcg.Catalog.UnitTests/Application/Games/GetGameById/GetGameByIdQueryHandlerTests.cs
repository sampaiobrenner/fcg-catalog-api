using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Games.GetGameById;
using Fcg.Catalog.UnitTests._Fixtures;

namespace Fcg.Catalog.UnitTests.Application.Games.GetGameById;

public class GetGameByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_DeveRetornarJogo_QuandoExiste()
    {
        await using var context = InMemoryCatalogDbContext.Create();
        var game = TestData.Game(49.90m, "Hades", "Ação");
        game.Deactivate();
        context.Add(game.PersistenceModel);
        await context.SaveChangesAsync();

        var response = await new GetGameByIdQueryHandler(context).Handle(new GetGameByIdQuery(game.Id()), CancellationToken.None);

        response.Id.Should().Be(game.Id());
        response.Title.Should().Be("Hades");
        response.Price.Should().Be(49.90m);
        response.Active.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoJogoInexistente()
    {
        await using var context = InMemoryCatalogDbContext.Create();

        var act = () => new GetGameByIdQueryHandler(context).Handle(new GetGameByIdQuery(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
