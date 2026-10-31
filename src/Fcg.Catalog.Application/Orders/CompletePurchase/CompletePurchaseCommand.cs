using Fcg.Contracts.Events.V1;
using MediatR;

namespace Fcg.Catalog.Application.Orders.CompletePurchase;

/// <summary>Resultado do pagamento (vindo de <see cref="PaymentProcessedEvent"/>) aplicado ao pedido.</summary>
public sealed record CompletePurchaseCommand(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    PaymentStatus Status,
    string? Reason) : IRequest;
