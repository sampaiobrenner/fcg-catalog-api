using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application._Shared.Security;
using Fcg.Catalog.Application.Properties;
using Fcg.Catalog.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Application.Orders.ListMyOrders;

internal sealed class ListMyOrdersQueryHandler : IRequestHandler<ListMyOrdersQuery, IReadOnlyCollection<OrderResponse>>
{
    private readonly ICatalogDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ListMyOrdersQueryHandler(ICatalogDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<OrderResponse>> Handle(ListMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException(ApplicationResources.UsuarioNaoAutenticado);

        return await _context.DataSet<OrderPersistenceModel>()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .Select(order => new OrderResponse(
                order.Id,
                order.UserId,
                order.GameId,
                order.Price,
                order.Status,
                order.Reason,
                order.CreatedAt,
                order.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
