using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application._Shared.Messaging;
using Fcg.Catalog.Application._Shared.Parsers;
using Fcg.Catalog.Application._Shared.Security;
using Fcg.Catalog.Application.Properties;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Library;
using Fcg.Catalog.Domain.Orders;
using Fcg.Catalog.Domain.Pricing;
using Fcg.Catalog.Domain.Promotions;
using Fcg.Contracts.Events.V1;
using MediatR;

namespace Fcg.Catalog.Application.Orders.PlaceOrder;

/// <summary>
/// Registra o pedido <c>Pending</c> com o preco efetivo e publica <see cref="OrderPlacedEvent"/>
/// pelo outbox (mesma transacao do pedido). O pagamento acontece na PaymentsAPI.
/// </summary>
internal sealed class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResponse>
{
    private readonly IGameRepository _games;
    private readonly IPromotionRepository _promotions;
    private readonly ILibraryRepository _library;
    private readonly IOrderRepository _orders;
    private readonly PricingService _pricing;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly ICatalogDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IParser<Order, PlaceOrderResponse> _parser;

    public PlaceOrderCommandHandler(
        IGameRepository games,
        IPromotionRepository promotions,
        ILibraryRepository library,
        IOrderRepository orders,
        PricingService pricing,
        IIntegrationEventPublisher publisher,
        ICatalogDbContext context,
        ICurrentUser currentUser,
        IParser<Order, PlaceOrderResponse> parser)
    {
        _games = games;
        _promotions = promotions;
        _library = library;
        _orders = orders;
        _pricing = pricing;
        _publisher = publisher;
        _context = context;
        _currentUser = currentUser;
        _parser = parser;
    }

    public async Task<PlaceOrderResponse> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException(ApplicationResources.UsuarioNaoAutenticado);

        var game = await _games.GetByIdAsync(request.GameId, cancellationToken);
        if (game is null || !game.Active)
            throw new NotFoundException(ApplicationResources.Jogo, request.GameId);

        if (await _library.ExistsAsync(userId, game.Id(), cancellationToken))
            throw new ConflictException(ApplicationResources.JogoJaNaBiblioteca);

        if (await _orders.HasPendingAsync(userId, game.Id(), cancellationToken))
            throw new ConflictException(ApplicationResources.PedidoEmAndamento);

        var now = DateTime.UtcNow;
        var promotions = await _promotions.GetActiveForGameAsync(game.Id(), now, cancellationToken);
        var price = _pricing.GetEffectivePrice(game, promotions, now);

        var order = Order.Place(userId, game.Id(), price);
        await _orders.AddAsync(order, cancellationToken);

        await _publisher.PublishAsync(
            new OrderPlacedEvent(Guid.CreateVersion7(), now, order.Id(), order.UserId, order.GameId, order.Price.Amount),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return _parser.Parse(order)!;
    }
}
