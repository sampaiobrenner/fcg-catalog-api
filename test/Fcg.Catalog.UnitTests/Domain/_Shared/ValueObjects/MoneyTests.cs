using Fcg.Catalog.Domain._Shared.Exceptions;
using Fcg.Catalog.Domain._Shared.ValueObjects;

namespace Fcg.Catalog.UnitTests.Domain._Shared.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_DeveArredondarParaDuasCasas_QuandoValorPositivo()
    {
        var money = Money.Create(10.555m);

        money.Amount.Should().Be(10.56m);
        money.Currency.Should().Be(Money.DefaultCurrency);
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoValorNegativo()
    {
        var act = () => Money.Create(-1m);

        act.Should().Throw<BusinessException>().WithMessage("*não pode ser negativo*");
    }

    [Theory]
    [InlineData(100, 10, 90)]
    [InlineData(100, 0, 100)]
    [InlineData(59.90, 50, 29.95)]
    [InlineData(79.90, 25, 59.93)]
    public void ApplyDiscount_DeveReduzirValor_QuandoPercentualValido(decimal amount, decimal percent, decimal expected)
    {
        Money.Create(amount).ApplyDiscount(percent).Amount.Should().Be(expected);
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(150)]
    public void ApplyDiscount_DeveLancarExcecao_QuandoPercentualInvalido(decimal percent)
    {
        var act = () => Money.Create(100m).ApplyDiscount(percent);

        act.Should().Throw<BusinessException>().WithMessage("*entre 0 e 100*");
    }

    [Fact]
    public void ApplyDiscount_DeveRetornarZero_QuandoDescontoTotal()
    {
        Money.Create(59.90m).ApplyDiscount(100).Amount.Should().Be(0m);
    }

    [Fact]
    public void Zero_DeveSerZeroNaMoedaPadrao()
    {
        Money.Zero.Amount.Should().Be(0m);
        Money.Zero.Currency.Should().Be(Money.DefaultCurrency);
    }

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData("Eur", "EUR")]
    public void Create_DeveNormalizarMoeda_QuandoMinuscula(string currency, string expected)
    {
        Money.Create(10m, currency).Currency.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_DeveLancarExcecao_QuandoMoedaVazia(string currency)
    {
        var act = () => Money.Create(10m, currency);

        act.Should().Throw<BusinessException>().WithMessage("*moeda é obrigatória*");
    }

    [Fact]
    public void Equals_DeveSerVerdadeiro_QuandoMesmoValorEMoeda()
    {
        var money = Money.Create(19.99m);
        var other = Money.Create(19.99m);

        money.Equals(other).Should().BeTrue();
        money.GetHashCode().Should().Be(other.GetHashCode());
    }

    [Fact]
    public void Equals_DeveSerFalso_QuandoMoedaDiferente()
    {
        Money.Create(19.99m, "BRL").Equals(Money.Create(19.99m, "USD")).Should().BeFalse();
    }
}
