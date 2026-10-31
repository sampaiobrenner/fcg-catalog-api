using MediatR;

namespace Fcg.Catalog.Application.Orders.PlaceOrder;

/// <summary>Inicia a compra de um jogo pelo usuario autenticado.</summary>
public sealed record PlaceOrderCommand(Guid GameId) : IRequest<PlaceOrderResponse>;
