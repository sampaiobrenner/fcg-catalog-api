using Fcg.Catalog.Application.Games.DeleteGame;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using Fcg.Catalog.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Catalog.WebApi.Games.DeleteGame;

internal sealed class DeleteGameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/games/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteGameCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(EndpointTags.Games)
            .WithName("DeleteGame")
            .WithSummary("Desativa um jogo (administrador).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
