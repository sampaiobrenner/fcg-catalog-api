using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Properties;
using Fcg.Catalog.Domain.Library;
using Fcg.Catalog.Domain.Orders;
using Fcg.Contracts.Events.V1;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Catalog.Application.Orders.CompletePurchase;

/// <summary>
/// Conclui (pagamento aprovado: item na biblioteca + <c>Completed</c>) ou recusa o pedido.
/// Idempotente por <c>OrderId</c>: pedido que ja saiu de <c>Pending</c> e ignorado.
/// </summary>
internal sealed class CompletePurchaseCommandHandler : IRequestHandler<CompletePurchaseCommand>
{
    private readonly IOrderRepository _orders;
    private readonly ILibraryRepository _library;
    private readonly ICatalogDbContext _context;
    private readonly ILogger<CompletePurchaseCommandHandler> _logger;

    public CompletePurchaseCommandHandler(
        IOrderRepository orders,
        ILibraryRepository library,
        ICatalogDbContext context,
        ILogger<CompletePurchaseCommandHandler> logger)
    {
        _orders = orders;
        _library = library;
        _context = context;
        _logger = logger;
    }

    public async Task Handle(CompletePurchaseCommand request, CancellationToken cancellationToken)
    {
        // Pedido inexistente e excecao comum: entra no retry e, esgotado, vai para a fila _error.
        var order = await _orders.GetByIdAsync(request.OrderId, cancellationToken)
                    ?? throw new NotFoundException(ApplicationResources.Pedido, request.OrderId);

        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogInformation("Pedido {OrderId} ja esta {Status}; resultado de pagamento {PaymentStatus} ignorado.",
                order.Id(), order.Status, request.Status);
            return;
        }

        if (request.Status == PaymentStatus.Approved)
        {
            var item = order.Complete();

            if (!await _library.ExistsAsync(order.UserId, order.GameId, cancellationToken))
                await _library.AddAsync(item, cancellationToken);
        }
        else
        {
            order.Reject(request.Reason);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pedido {OrderId} atualizado para {Status}.", order.Id(), order.Status);
    }
}
