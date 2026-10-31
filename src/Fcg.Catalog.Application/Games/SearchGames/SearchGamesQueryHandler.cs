using Fcg.Catalog.Application._Shared.ReadQueries;
using MediatR;

namespace Fcg.Catalog.Application.Games.SearchGames;

internal sealed class SearchGamesQueryHandler : IRequestHandler<SearchGamesQuery, IReadOnlyCollection<GameCatalogItemResponse>>
{
    private readonly IGameReadQueries _readQueries;

    public SearchGamesQueryHandler(IGameReadQueries readQueries) => _readQueries = readQueries;

    public Task<IReadOnlyCollection<GameCatalogItemResponse>> Handle(SearchGamesQuery request, CancellationToken cancellationToken)
        => _readQueries.SearchCatalogAsync(Normalize(request), cancellationToken);

    private static SearchGamesQuery Normalize(SearchGamesQuery query) => query with
    {
        Title = string.IsNullOrWhiteSpace(query.Title) ? null : query.Title.Trim(),
        Genre = string.IsNullOrWhiteSpace(query.Genre) ? null : query.Genre.Trim(),
        Page = query.Page < 1 ? SearchGamesQuery.DefaultPage : query.Page,
        PageSize = query.PageSize is < 1 or > SearchGamesQuery.MaxPageSize ? SearchGamesQuery.DefaultPageSize : query.PageSize
    };
}
