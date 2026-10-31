using Fcg.Catalog.Domain._Shared.Exceptions;
using Fcg.Catalog.Domain.Properties;

namespace Fcg.Catalog.Domain._Shared.ValueObjects;

/// <summary>Valor monetario nao negativo, arredondado a 2 casas (MidpointRounding.AwayFromZero).</summary>
public sealed class Money : IEquatable<Money>
{
    public const string DefaultCurrency = "BRL";
    private const int Decimals = 2;

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money Zero => new(0m, DefaultCurrency);

    public static Money Create(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
            throw new BusinessException(DomainResources.ValorMonetarioNegativo);

        if (string.IsNullOrWhiteSpace(currency))
            throw new BusinessException(DomainResources.MoedaObrigatoria);

        return new Money(decimal.Round(amount, Decimals, MidpointRounding.AwayFromZero), currency.Trim().ToUpperInvariant());
    }

    /// <summary>Aplica um desconto percentual (0 a 100) e devolve um novo valor.</summary>
    public Money ApplyDiscount(decimal percent)
    {
        if (percent is < 0 or > 100)
            throw new BusinessException(DomainResources.DescontoInvalido);

        return Create(Amount * (1 - percent / 100m), Currency);
    }

    public bool Equals(Money? other) => other is not null && other.Amount == Amount && other.Currency == Currency;

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => $"{Currency} {Amount:0.00}";
}
