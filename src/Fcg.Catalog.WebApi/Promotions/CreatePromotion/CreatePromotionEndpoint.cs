using Fcg.Catalog.Application.Promotions;
using Fcg.Catalog.Application.Promotions.CreatePromotion;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using Fcg.Catalog.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Catalog.WebApi.Promotions.CreatePromotion;

internal sealed class CreatePromotionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/promotions", async (CreatePromotionCommand command, ISender sender, CancellationToken cancellationToken) =>
            {
                var promotion = await sender.Send(command, cancellationToken);
                return Results.Created($"/api/v1/promotions/{promotion.Id}", promotion);
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(EndpointTags.Promotions)
            .WithName("CreatePromotion")
            .WithSummary("Cria uma promocao de desconto para um conjunto de jogos (administrador).")
            .Produces<PromotionResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
