using Fcg.Catalog.Domain.Pricing;
using Fcg.Catalog.Domain.Promotions;
using Fcg.Catalog.UnitTests._Fixtures;

namespace Fcg.Catalog.UnitTests.Domain.Pricing;

public class PricingServiceTests
{
    private readonly PricingService _service = new();

    [Fact]
    public void GetEffectivePrice_DeveRetornarPrecoCheio_QuandoSemPromocao()
    {
        var game = TestData.Game(79.90m);

        _service.GetEffectivePrice(game, (Promotion?)null).Amount.Should().Be(79.90m);
    }

    [Fact]
    public void GetEffectivePrice_DeveAplicarDesconto_QuandoPromocaoAtiva()
    {
        var game = TestData.Game(79.90m);

        _service.GetEffectivePrice(game, TestData.ActivePromotion(game.Id(), 25m)).Amount.Should().Be(59.93m);
    }

    [Fact]
    public void GetEffectivePrice_DeveIgnorarPromocao_QuandoNaoIncluiOJogo()
    {
        var game = TestData.Game(50m);

        _service.GetEffectivePrice(game, TestData.ActivePromotion(Guid.CreateVersion7(), 50m)).Amount.Should().Be(50m);
    }

    [Fact]
    public void GetEffectivePrice_DeveUsarMaiorDescontoAtivo_QuandoHaVariasPromocoes()
    {
        var game = TestData.Game(100m);
        var expired = Promotion.Create("Expirada", 90m, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-5), [game.Id()]);
        var inactive = TestData.ActivePromotion(game.Id(), 80m);
        inactive.Deactivate();
        var otherGame = TestData.ActivePromotion(Guid.CreateVersion7(), 70m);

        var candidates = new[]
        {
            TestData.ActivePromotion(game.Id(), 10m),
            TestData.ActivePromotion(game.Id(), 30m),
            expired,
            inactive,
            otherGame
        };

        _service.GetEffectivePrice(game, candidates, DateTime.UtcNow).Amount.Should().Be(70m);
    }

    [Fact]
    public void FindBestPromotion_DeveRetornarNulo_QuandoNenhumaCandidataValida()
    {
        _service.FindBestPromotion(Guid.CreateVersion7(), [], DateTime.UtcNow).Should().BeNull();
    }
}
