using Fcg.Catalog.Application.Orders;
using Fcg.Catalog.Application.Orders.ListMyOrders;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using MediatR;

namespace Fcg.Catalog.WebApi.Orders.ListMyOrders;

internal sealed class ListMyOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/orders", async (ISender sender, CancellationToken cancellationToken)
                => Results.Ok(await sender.Send(new ListMyOrdersQuery(), cancellationToken)))
            .RequireAuthorization()
            .WithTags(EndpointTags.Orders)
            .WithName("ListMyOrders")
            .WithSummary("Lista os pedidos do usuario autenticado, mais recentes primeiro.")
            .Produces<IReadOnlyCollection<OrderResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
