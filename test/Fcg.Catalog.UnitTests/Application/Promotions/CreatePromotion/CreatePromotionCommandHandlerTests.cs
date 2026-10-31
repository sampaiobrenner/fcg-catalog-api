using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Promotions;
using Fcg.Catalog.Application.Promotions.CreatePromotion;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Promotions;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Promotions.CreatePromotion;

public class CreatePromotionCommandHandlerTests
{
    private readonly Mock<IPromotionRepository> _promotions = new();
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<ICatalogDbContext> _context = new();

    private CreatePromotionCommandHandler CreateSut()
        => new(_promotions.Object, _games.Object, _context.Object, new PromotionResponseParser());

    private static CreatePromotionCommand Command(params Guid[] gameIds)
        => new("Black Friday", 30m, DateTime.UtcNow, DateTime.UtcNow.AddDays(7), gameIds);

    [Fact]
    public async Task Handle_DeveCriarPromocao_QuandoJogosExistem()
    {
        var gameId = Guid.CreateVersion7();
        _games.Setup(r => r.GetExistingIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([gameId]);

        var response = await CreateSut().Handle(Command(gameId, gameId), CancellationToken.None);

        response.Name.Should().Be("Black Friday");
        response.DiscountPercent.Should().Be(30m);
        response.Active.Should().BeTrue();
        response.GameIds.Should().Equal(gameId);
        _promotions.Verify(r => r.AddAsync(It.IsAny<Promotion>(), It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoJogoInexistente()
    {
        var existing = Guid.CreateVersion7();
        var missing = Guid.CreateVersion7();
        _games.Setup(r => r.GetExistingIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([existing]);

        var act = () => CreateSut().Handle(Command(existing, missing), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{missing}*");
        _promotions.Verify(r => r.AddAsync(It.IsAny<Promotion>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
