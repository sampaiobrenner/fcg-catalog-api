using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Promotions.DeletePromotion;
using Fcg.Catalog.Domain.Promotions;
using Fcg.Catalog.UnitTests._Fixtures;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Promotions.DeletePromotion;

public class DeletePromotionCommandHandlerTests
{
    private readonly Mock<IPromotionRepository> _promotions = new();
    private readonly Mock<ICatalogDbContext> _context = new();

    [Fact]
    public async Task Handle_DeveDesativarPromocao_QuandoExiste()
    {
        var promotion = TestData.ActivePromotion(Guid.CreateVersion7());
        _promotions.Setup(r => r.GetByIdAsync(promotion.Id(), It.IsAny<CancellationToken>())).ReturnsAsync(promotion);

        await new DeletePromotionCommandHandler(_promotions.Object, _context.Object).Handle(new DeletePromotionCommand(promotion.Id()), CancellationToken.None);

        promotion.Active.Should().BeFalse();
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoPromocaoInexistente()
    {
        _promotions.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Promotion?)null);

        var act = () => new DeletePromotionCommandHandler(_promotions.Object, _context.Object)
            .Handle(new DeletePromotionCommand(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
