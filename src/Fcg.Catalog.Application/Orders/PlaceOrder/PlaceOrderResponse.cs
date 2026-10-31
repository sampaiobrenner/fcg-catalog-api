using Fcg.Catalog.Domain.Orders;

namespace Fcg.Catalog.Application.Orders.PlaceOrder;

public sealed record PlaceOrderResponse(Guid OrderId, OrderStatus Status, decimal Price);
