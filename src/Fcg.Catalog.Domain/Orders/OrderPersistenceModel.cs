using Fcg.Catalog.Domain._Shared.Models;

namespace Fcg.Catalog.Domain.Orders;

public sealed class OrderPersistenceModel : PersistenceModelBase
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public decimal Price { get; set; }
    public OrderStatus Status { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
