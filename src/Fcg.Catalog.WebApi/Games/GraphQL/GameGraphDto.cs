namespace Fcg.Catalog.WebApi.Games.GraphQL;

/// <summary>Projecao de jogo exposta via GraphQL (permite filtragem e ordenacao dinamicas).</summary>
public sealed class GameGraphDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public DateOnly? ReleaseDate { get; init; }
}
