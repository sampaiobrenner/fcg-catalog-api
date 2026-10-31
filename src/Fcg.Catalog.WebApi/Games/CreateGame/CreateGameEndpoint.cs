using Fcg.Catalog.Application.Games;
using Fcg.Catalog.Application.Games.CreateGame;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using Fcg.Catalog.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Catalog.WebApi.Games.CreateGame;

internal sealed class CreateGameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/games", async (CreateGameCommand command, ISender sender, CancellationToken cancellationToken) =>
            {
                var game = await sender.Send(command, cancellationToken);
                return Results.Created($"/api/v1/games/{game.Id}", game);
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(EndpointTags.Games)
            .WithName("CreateGame")
            .WithSummary("Cadastra um novo jogo (administrador).")
            .Produces<GameResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
