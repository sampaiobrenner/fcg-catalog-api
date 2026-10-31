using Fcg.Contracts.Events.V1;
using MassTransit;

namespace Fcg.Catalog.IntegrationTests._Fixtures;

/// <summary>
/// Faz o papel da PaymentsAPI nos testes: consome o <see cref="OrderPlacedEvent"/> publicado para que o JSON que
/// trafegou no broker possa ser comparado com o sample do contrato.
/// </summary>
public sealed class OrderPlacedProbeConsumer : IConsumer<OrderPlacedEvent>
{
    public Task Consume(ConsumeContext<OrderPlacedEvent> context) => Task.CompletedTask;
}
