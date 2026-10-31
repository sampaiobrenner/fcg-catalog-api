using MediatR;

namespace Fcg.Catalog.Application.Games.SearchGames;

public sealed record SearchGamesQuery(
    string? Title = null,
    string? Genre = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int Page = SearchGamesQuery.DefaultPage,
    int PageSize = SearchGamesQuery.DefaultPageSize) : IRequest<IReadOnlyCollection<GameCatalogItemResponse>>
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}
