using Fcg.Catalog.Application.Orders.CompletePurchase;
using Fcg.Contracts.Events.V1;
using MassTransit;
using MediatR;

namespace Fcg.Catalog.WebApi.Orders.CompletePurchase;

/// <summary>Recebe o resultado do pagamento (PaymentsAPI) e delega a conclusao da compra ao MediatR.</summary>
public sealed class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly ISender _sender;

    public PaymentProcessedConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var message = context.Message;

        return _sender.Send(
            new CompletePurchaseCommand(message.OrderId, message.UserId, message.GameId, message.Price, message.Status, message.Reason),
            context.CancellationToken);
    }
}
