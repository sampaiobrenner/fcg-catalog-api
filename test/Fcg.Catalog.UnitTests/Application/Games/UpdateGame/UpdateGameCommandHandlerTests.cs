using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Games;
using Fcg.Catalog.Application.Games.UpdateGame;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.UnitTests._Fixtures;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Games.UpdateGame;

public class UpdateGameCommandHandlerTests
{
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<ICatalogDbContext> _context = new();

    private UpdateGameCommandHandler CreateSut() => new(_games.Object, _context.Object, new GameResponseParser());

    [Fact]
    public async Task Handle_DeveAtualizarJogo_QuandoExiste()
    {
        var game = TestData.Game(19.90m, "Portal");
        _games.Setup(r => r.GetByIdAsync(game.Id(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var response = await CreateSut().Handle(
            new UpdateGameCommand(game.Id(), "Portal 2", "Sequel", "Puzzle", 39.90m, new DateOnly(2011, 4, 18)),
            CancellationToken.None);

        response.Title.Should().Be("Portal 2");
        response.Price.Should().Be(39.90m);
        game.PersistenceModel.Title.Should().Be("Portal 2");
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoJogoInexistente()
    {
        _games.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Game?)null);

        var act = () => CreateSut().Handle(new UpdateGameCommand(Guid.CreateVersion7(), "X", null, "Y", 10m, null), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
