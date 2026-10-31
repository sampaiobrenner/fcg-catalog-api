using Fcg.Catalog.Application.Orders.PlaceOrder;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using MediatR;

namespace Fcg.Catalog.WebApi.Orders.PlaceOrder;

internal sealed class PlaceOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/orders", async (PlaceOrderCommand command, ISender sender, CancellationToken cancellationToken) =>
            {
                var order = await sender.Send(command, cancellationToken);
                return Results.Accepted($"/api/v1/orders/{order.OrderId}", order);
            })
            .RequireAuthorization()
            .WithTags(EndpointTags.Orders)
            .WithName("PlaceOrder")
            .WithSummary("Inicia a compra de um jogo: registra o pedido Pending e publica OrderPlacedEvent.")
            .Produces<PlaceOrderResponse>(StatusCodes.Status202Accepted)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
