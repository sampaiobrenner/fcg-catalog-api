namespace Fcg.Catalog.Application.Library.GetMyLibrary;

/// <summary>Jogo da biblioteca do usuario (leitura Dapper).</summary>
public sealed record LibraryItemResponse
{
    public Guid GameId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public decimal PricePaid { get; init; }
    public DateTime AcquiredAt { get; init; }
}
