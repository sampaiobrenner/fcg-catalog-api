using MediatR;

namespace Fcg.Catalog.Application.Orders.ListMyOrders;

/// <summary>Pedidos do usuario autenticado, mais recentes primeiro.</summary>
public sealed record ListMyOrdersQuery : IRequest<IReadOnlyCollection<OrderResponse>>;
