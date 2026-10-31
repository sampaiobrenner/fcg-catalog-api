using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Games.DeleteGame;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.UnitTests._Fixtures;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Games.DeleteGame;

public class DeleteGameCommandHandlerTests
{
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<ICatalogDbContext> _context = new();

    private DeleteGameCommandHandler CreateSut() => new(_games.Object, _context.Object);

    [Fact]
    public async Task Handle_DeveDesativarJogo_QuandoExiste()
    {
        var game = TestData.Game();
        _games.Setup(r => r.GetByIdAsync(game.Id(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        await CreateSut().Handle(new DeleteGameCommand(game.Id()), CancellationToken.None);

        game.Active.Should().BeFalse();
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoJogoInexistente()
    {
        _games.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Game?)null);

        var act = () => CreateSut().Handle(new DeleteGameCommand(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
