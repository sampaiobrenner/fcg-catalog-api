using Fcg.Catalog.Application.Orders;
using Fcg.Catalog.Application.Orders.GetOrderById;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using MediatR;

namespace Fcg.Catalog.WebApi.Orders.GetOrderById;

internal sealed class GetOrderByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/orders/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken)
                => Results.Ok(await sender.Send(new GetOrderByIdQuery(id), cancellationToken)))
            .RequireAuthorization()
            .WithTags(EndpointTags.Orders)
            .WithName("GetOrderById")
            .WithSummary("Detalha um pedido (dono ou administrador).")
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
