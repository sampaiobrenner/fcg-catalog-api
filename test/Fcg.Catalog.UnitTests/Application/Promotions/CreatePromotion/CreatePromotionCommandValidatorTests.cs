using Fcg.Catalog.Application.Promotions.CreatePromotion;

namespace Fcg.Catalog.UnitTests.Application.Promotions.CreatePromotion;

public class CreatePromotionCommandValidatorTests
{
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly CreatePromotionCommandValidator _validator = new();

    [Fact]
    public void Validate_DeveAceitar_QuandoDadosValidos()
    {
        _validator.Validate(new CreatePromotionCommand("Natal", 10m, Start, Start.AddDays(5), [Guid.CreateVersion7()]))
            .IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(100.01)]
    public void Validate_DeveRejeitar_QuandoDescontoInvalido(decimal discount)
    {
        var result = _validator.Validate(new CreatePromotionCommand("Natal", discount, Start, Start.AddDays(5), [Guid.CreateVersion7()]));

        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreatePromotionCommand.DiscountPercent));
    }

    [Fact]
    public void Validate_DeveRejeitar_QuandoDatasInvertidas()
    {
        var result = _validator.Validate(new CreatePromotionCommand("Natal", 10m, Start, Start.AddDays(-1), [Guid.CreateVersion7()]));

        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreatePromotionCommand.EndsAt)
                                                && error.ErrorMessage.Contains("posterior"));
    }

    [Fact]
    public void Validate_DeveRejeitar_QuandoSemJogos()
    {
        var result = _validator.Validate(new CreatePromotionCommand("Natal", 10m, Start, Start.AddDays(1), []));

        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreatePromotionCommand.GameIds));
    }
}
