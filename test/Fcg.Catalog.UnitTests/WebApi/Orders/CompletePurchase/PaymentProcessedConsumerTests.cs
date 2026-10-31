using Fcg.Catalog.Application.Orders.CompletePurchase;
using Fcg.Catalog.WebApi.Orders.CompletePurchase;
using Fcg.Contracts.Events.V1;
using Fcg.Contracts.Messaging;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Fcg.Catalog.UnitTests.WebApi.Orders.CompletePurchase;

public class PaymentProcessedConsumerTests
{
    [Fact]
    public async Task Consume_DeveEnviarCommandComDadosDoEvento()
    {
        var message = new PaymentProcessedEvent(Guid.CreateVersion7(), DateTime.UtcNow, Guid.CreateVersion7(), Guid.CreateVersion7(),
            Guid.CreateVersion7(), 99.90m, PaymentStatus.Rejected, "limite");
        var context = new Mock<ConsumeContext<PaymentProcessedEvent>>();
        context.Setup(c => c.Message).Returns(message);
        context.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        var sender = new Mock<ISender>();

        await new PaymentProcessedConsumer(sender.Object).Consume(context.Object);

        sender.Verify(s => s.Send(
            new CompletePurchaseCommand(message.OrderId, message.UserId, message.GameId, 99.90m, PaymentStatus.Rejected, "limite"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Definition_DeveUsarFilaDoContrato_QuandoSemConfiguracao()
    {
        IConsumerDefinition definition = new PaymentProcessedConsumerDefinition(new ConfigurationBuilder().Build());

        definition.GetEndpointName(KebabCaseEndpointNameFormatter.Instance)
            .Should().Be(QueueNames.CatalogPaymentProcessed).And.Be("catalog.payment-processed");
    }

    [Fact]
    public void Definition_DeveUsarFilaDoConfigMap_QuandoConfigurada()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Messaging:Queues:CatalogPaymentProcessed"] = "catalog.payment-processed.custom" })
            .Build();

        IConsumerDefinition definition = new PaymentProcessedConsumerDefinition(configuration);

        definition.GetEndpointName(KebabCaseEndpointNameFormatter.Instance).Should().Be("catalog.payment-processed.custom");
    }
}
