namespace Fcg.Catalog.Application.Games;

public sealed record GameResponse(
    Guid Id,
    string Title,
    string Description,
    string Genre,
    decimal Price,
    DateOnly? ReleaseDate,
    bool Active,
    DateTime CreatedAt);
