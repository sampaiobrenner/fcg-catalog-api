using MediatR;

namespace Fcg.Catalog.Application.Orders.GetOrderById;

/// <summary>Detalhe do pedido; visivel apenas ao dono ou a um administrador.</summary>
public sealed record GetOrderByIdQuery(Guid Id) : IRequest<OrderResponse>;
