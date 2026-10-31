using Fcg.Catalog.Domain.Orders;

namespace Fcg.Catalog.Application.Orders;

public sealed record OrderResponse(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    OrderStatus Status,
    string? Reason,
    DateTime CreatedAt,
    DateTime UpdatedAt);
