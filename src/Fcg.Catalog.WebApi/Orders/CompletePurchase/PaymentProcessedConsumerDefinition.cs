using Fcg.Contracts.Messaging;
using MassTransit;

namespace Fcg.Catalog.WebApi.Orders.CompletePurchase;

/// <summary>
/// Fila <c>catalog.payment-processed</c>. O nome vem do ConfigMap (<c>Messaging__Queues__CatalogPaymentProcessed</c>),
/// com a constante do pacote de contratos como default.
/// </summary>
public sealed class PaymentProcessedConsumerDefinition : ConsumerDefinition<PaymentProcessedConsumer>
{
    public const string QueueConfigurationKey = $"Messaging:Queues:{nameof(QueueNames.CatalogPaymentProcessed)}";

    public PaymentProcessedConsumerDefinition(IConfiguration configuration)
    {
        var configured = configuration[QueueConfigurationKey];
        EndpointName = string.IsNullOrWhiteSpace(configured) ? QueueNames.CatalogPaymentProcessed : configured;
    }
}
