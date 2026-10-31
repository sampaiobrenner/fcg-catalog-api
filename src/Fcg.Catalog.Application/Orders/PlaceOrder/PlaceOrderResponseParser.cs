using Fcg.Catalog.Application._Shared.Parsers;
using Fcg.Catalog.Domain.Orders;

namespace Fcg.Catalog.Application.Orders.PlaceOrder;

internal sealed class PlaceOrderResponseParser : Parser<Order, PlaceOrderResponse>
{
    protected override PlaceOrderResponse DoParse(Order source) => new(source.Id(), source.Status, source.Price.Amount);
}
