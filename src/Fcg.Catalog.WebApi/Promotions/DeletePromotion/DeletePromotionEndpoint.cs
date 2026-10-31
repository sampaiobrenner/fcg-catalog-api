using Fcg.Catalog.Application.Promotions.DeletePromotion;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using Fcg.Catalog.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Catalog.WebApi.Promotions.DeletePromotion;

internal sealed class DeletePromotionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/promotions/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeletePromotionCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(EndpointTags.Promotions)
            .WithName("DeletePromotion")
            .WithSummary("Desativa uma promocao (administrador).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
