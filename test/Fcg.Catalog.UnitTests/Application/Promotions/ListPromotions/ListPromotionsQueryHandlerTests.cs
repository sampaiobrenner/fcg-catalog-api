using Fcg.Catalog.Application.Promotions.ListPromotions;
using Fcg.Catalog.UnitTests._Fixtures;

namespace Fcg.Catalog.UnitTests.Application.Promotions.ListPromotions;

public class ListPromotionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_DeveListarPromocoesComJogos()
    {
        await using var context = InMemoryCatalogDbContext.Create();
        var gameId = Guid.CreateVersion7();
        var promotion = TestData.ActivePromotion(gameId, 15m, "Inverno");
        context.Add(promotion.PersistenceModel);
        await context.SaveChangesAsync();

        var result = await new ListPromotionsQueryHandler(context).Handle(new ListPromotionsQuery(), CancellationToken.None);

        var item = result.Should().ContainSingle().Subject;
        item.Id.Should().Be(promotion.Id());
        item.Name.Should().Be("Inverno");
        item.DiscountPercent.Should().Be(15m);
        item.GameIds.Should().Equal(gameId);
    }
}
