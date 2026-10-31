using Fcg.Catalog.Application.Games;
using Fcg.Catalog.Application.Games.UpdateGame;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using Fcg.Catalog.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Catalog.WebApi.Games.UpdateGame;

internal sealed class UpdateGameEndpoint : IEndpoint
{
    public sealed record UpdateGameRequest(string Title, string? Description, string Genre, decimal Price, DateOnly? ReleaseDate);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/games/{id:guid}", async (Guid id, UpdateGameRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new UpdateGameCommand(id, request.Title, request.Description, request.Genre, request.Price, request.ReleaseDate);
                return Results.Ok(await sender.Send(command, cancellationToken));
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(EndpointTags.Games)
            .WithName("UpdateGame")
            .WithSummary("Atualiza um jogo (administrador).")
            .Produces<GameResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
