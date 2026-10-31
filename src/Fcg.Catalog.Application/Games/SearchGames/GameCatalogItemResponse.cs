namespace Fcg.Catalog.Application.Games.SearchGames;

/// <summary>Item do catalogo com o preco efetivo apos a promocao ativa de maior desconto (leitura Dapper).</summary>
public sealed record GameCatalogItemResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? DiscountPercent { get; init; }
    public decimal EffectivePrice { get; init; }
    public DateOnly? ReleaseDate { get; init; }
}
