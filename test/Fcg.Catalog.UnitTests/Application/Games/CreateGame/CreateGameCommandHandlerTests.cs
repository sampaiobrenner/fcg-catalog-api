using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Games;
using Fcg.Catalog.Application.Games.CreateGame;
using Fcg.Catalog.Domain.Games;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Games.CreateGame;

public class CreateGameCommandHandlerTests
{
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<ICatalogDbContext> _context = new();

    private CreateGameCommandHandler CreateSut() => new(_games.Object, _context.Object, new GameResponseParser());

    [Fact]
    public async Task Handle_DevePersistirJogo_QuandoTituloUnico()
    {
        _games.Setup(r => r.TitleExistsAsync("Portal 2", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var response = await CreateSut().Handle(new CreateGameCommand(" Portal 2 ", "Puzzle", "Puzzle", 39.90m, null), CancellationToken.None);

        response.Title.Should().Be("Portal 2");
        response.Price.Should().Be(39.90m);
        response.Active.Should().BeTrue();
        _games.Verify(r => r.AddAsync(It.Is<Game>(game => game.Id() == response.Id), It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarConflito_QuandoTituloDuplicado()
    {
        _games.Setup(r => r.TitleExistsAsync("Portal 2", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => CreateSut().Handle(new CreateGameCommand("Portal 2", "Puzzle", "Puzzle", 39.90m, null), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*Portal 2*");
        _games.Verify(r => r.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Never);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
