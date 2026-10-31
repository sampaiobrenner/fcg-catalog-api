namespace Fcg.Catalog.Domain.Orders;

/// <summary>Situacao do pedido de compra (gravada como texto no banco e no JSON).</summary>
public enum OrderStatus
{
    Pending = 1,
    Completed = 2,
    Rejected = 3
}
