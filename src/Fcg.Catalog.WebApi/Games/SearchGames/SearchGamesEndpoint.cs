using Fcg.Catalog.Application.Games.SearchGames;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using MediatR;

namespace Fcg.Catalog.WebApi.Games.SearchGames;

internal sealed class SearchGamesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/games", async (
                string? title,
                string? genre,
                decimal? minPrice,
                decimal? maxPrice,
                int? page,
                int? pageSize,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new SearchGamesQuery(
                    title,
                    genre,
                    minPrice,
                    maxPrice,
                    page ?? SearchGamesQuery.DefaultPage,
                    pageSize ?? SearchGamesQuery.DefaultPageSize);

                return Results.Ok(await sender.Send(query, cancellationToken));
            })
            .AllowAnonymous()
            .WithTags(EndpointTags.Games)
            .WithName("SearchGames")
            .WithSummary("Lista o catalogo de jogos ativos com filtros e preco promocional efetivo.")
            .Produces<IReadOnlyCollection<GameCatalogItemResponse>>()
            .ProducesValidationProblem();
    }
}
