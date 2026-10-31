using Fcg.Catalog.Domain.Orders;
using Fcg.Catalog.Infrastructure._Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Infrastructure.Orders;

internal sealed class OrderRepository : IOrderRepository
{
    private readonly CatalogDbContext _context;

    public OrderRepository(CatalogDbContext context) => _context = context;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await _context.Set<OrderPersistenceModel>().FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
        return model is null ? null : new Order(model);
    }

    public Task<bool> HasPendingAsync(Guid userId, Guid gameId, CancellationToken cancellationToken)
        => _context.Set<OrderPersistenceModel>()
            .AnyAsync(order => order.UserId == userId && order.GameId == gameId && order.Status == OrderStatus.Pending, cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
        => await _context.Set<OrderPersistenceModel>().AddAsync(order.PersistenceModel, cancellationToken);
}
