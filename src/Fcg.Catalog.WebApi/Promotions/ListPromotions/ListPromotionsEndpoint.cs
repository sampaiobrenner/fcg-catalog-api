using Fcg.Catalog.Application.Promotions;
using Fcg.Catalog.Application.Promotions.ListPromotions;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using Fcg.Catalog.WebApi._Shared.Security;
using MediatR;

namespace Fcg.Catalog.WebApi.Promotions.ListPromotions;

internal sealed class ListPromotionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/promotions", async (ISender sender, CancellationToken cancellationToken)
                => Results.Ok(await sender.Send(new ListPromotionsQuery(), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags(EndpointTags.Promotions)
            .WithName("ListPromotions")
            .WithSummary("Lista as promocoes (administrador).")
            .Produces<IReadOnlyCollection<PromotionResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
