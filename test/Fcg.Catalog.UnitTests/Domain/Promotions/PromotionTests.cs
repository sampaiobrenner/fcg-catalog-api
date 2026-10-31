using Fcg.Catalog.Domain._Shared.Exceptions;
using Fcg.Catalog.Domain.Promotions;

namespace Fcg.Catalog.UnitTests.Domain.Promotions;

public class PromotionTests
{
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    private static Promotion NewPromotion(decimal discount = 20m, params Guid[] gameIds)
        => Promotion.Create("Summer Sale", discount, Start, End, gameIds.Length == 0 ? [Guid.CreateVersion7()] : gameIds);

    [Fact]
    public void Create_DeveCriarPromocaoAtiva_QuandoDadosValidos()
    {
        var gameId = Guid.CreateVersion7();

        var promotion = NewPromotion(15m, gameId, gameId);

        promotion.Active.Should().BeTrue();
        promotion.DiscountPercent.Should().Be(15m);
        promotion.GameIds.Should().ContainSingle().Which.Should().Be(gameId);
        promotion.PersistenceModel.Games.Should().OnlyContain(game => game.PromotionId == promotion.Id());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(120)]
    public void Create_DeveLancarExcecao_QuandoDescontoInvalido(decimal discount)
    {
        var act = () => NewPromotion(discount);

        act.Should().Throw<BusinessException>().WithMessage("*entre 0 e 100*");
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoTerminoAntesDoInicio()
    {
        var act = () => Promotion.Create("X", 10m, End, Start, [Guid.CreateVersion7()]);

        act.Should().Throw<BusinessException>().WithMessage("*posterior*");
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoSemJogos()
    {
        var act = () => Promotion.Create("X", 10m, Start, End, []);

        act.Should().Throw<BusinessException>().WithMessage("*ao menos um jogo*");
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoNomeVazio()
    {
        var act = () => Promotion.Create(" ", 10m, Start, End, [Guid.CreateVersion7()]);

        act.Should().Throw<BusinessException>().WithMessage("*nome*");
    }

    [Fact]
    public void Create_DeveConverterDatasParaUtc_QuandoSemKind()
    {
        var promotion = Promotion.Create("X", 10m, new DateTime(2026, 1, 1), new DateTime(2026, 2, 1), [Guid.CreateVersion7()]);

        promotion.StartsAt.Kind.Should().Be(DateTimeKind.Utc);
        promotion.EndsAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void AppliesTo_DeveIndicarSeJogoParticipa()
    {
        var gameId = Guid.CreateVersion7();
        var promotion = NewPromotion(10m, gameId);

        promotion.AppliesTo(gameId).Should().BeTrue();
        promotion.AppliesTo(Guid.CreateVersion7()).Should().BeFalse();
    }

    [Fact]
    public void IsActiveOn_DeveRespeitarPeriodoEStatus()
    {
        var promotion = NewPromotion();

        promotion.IsActiveOn(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)).Should().BeTrue();
        promotion.IsActiveOn(new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)).Should().BeFalse();
        promotion.IsActiveOn(new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc)).Should().BeFalse();

        promotion.Deactivate();
        promotion.IsActiveOn(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)).Should().BeFalse();
    }
}
