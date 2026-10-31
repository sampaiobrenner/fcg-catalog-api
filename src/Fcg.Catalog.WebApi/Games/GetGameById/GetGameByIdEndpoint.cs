using Fcg.Catalog.Application.Games;
using Fcg.Catalog.Application.Games.GetGameById;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using MediatR;

namespace Fcg.Catalog.WebApi.Games.GetGameById;

internal sealed class GetGameByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/games/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken)
                => Results.Ok(await sender.Send(new GetGameByIdQuery(id), cancellationToken)))
            .AllowAnonymous()
            .WithTags(EndpointTags.Games)
            .WithName("GetGameById")
            .WithSummary("Detalha um jogo.")
            .Produces<GameResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
