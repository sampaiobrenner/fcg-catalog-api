using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application._Shared.Security;
using Fcg.Catalog.Application.Properties;
using Fcg.Catalog.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Application.Orders.GetOrderById;

internal sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse>
{
    private readonly ICatalogDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetOrderByIdQueryHandler(ICatalogDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<OrderResponse> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException(ApplicationResources.UsuarioNaoAutenticado);

        var order = await _context.DataSet<OrderPersistenceModel>()
            .Where(order => order.Id == request.Id)
            .Select(order => new OrderResponse(
                order.Id,
                order.UserId,
                order.GameId,
                order.Price,
                order.Status,
                order.Reason,
                order.CreatedAt,
                order.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        // Pedido de outro usuario responde 404 para nao revelar a existencia.
        if (order is null || (order.UserId != userId && !_currentUser.IsAdministrator))
            throw new NotFoundException(ApplicationResources.Pedido, request.Id);

        return order;
    }
}
