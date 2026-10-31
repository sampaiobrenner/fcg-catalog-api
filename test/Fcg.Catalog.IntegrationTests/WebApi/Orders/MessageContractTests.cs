using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fcg.Catalog.Application.Orders.PlaceOrder;
using Fcg.Catalog.Domain.Orders;
using Fcg.Catalog.IntegrationTests._Fixtures;
using Fcg.Contracts.Events.V1;
using Fcg.Contracts.Messaging;
using MassTransit;
using MassTransit.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.IntegrationTests.WebApi.Orders;

/// <summary>Garante que o JSON trafegado no broker segue os samples do fcg-contracts (enum string, decimal numerico).</summary>
[Collection(CatalogApiCollection.Name)]
public class MessageContractTests
{
    private readonly CatalogApiFactory _factory;

    public MessageContractTests(CatalogApiFactory factory) => _factory = factory;

    [Fact]
    public async Task OrderPlaced_DevePublicarPayloadDoContrato()
    {
        var userId = Guid.CreateVersion7();
        var game = await _factory.CreateAdminClient().CreateGameAsync(199.90m);
        var order = await (await _factory.CreateUserClient(userId).PostAsJsonAsync("/api/v1/orders", new { gameId = game.Id }))
            .ReadAsAsync<PlaceOrderResponse>();

        (await Eventually.TrueAsync(() => Task.FromResult(_factory.Harness.Consumed
            .Select<OrderPlacedEvent>(context => context.Context.Message.OrderId == order.OrderId).Any()))).Should().BeTrue();
        var received = _factory.Harness.Consumed.Select<OrderPlacedEvent>(context => context.Context.Message.OrderId == order.OrderId).First();

        // JSON exatamente como trafegou no broker (envelope MassTransit).
        var envelope = JsonNode.Parse(received.Context.ReceiveContext.Body.GetString())!.AsObject();
        envelope["messageType"]!.AsArray().Select(type => type!.GetValue<string>()).Should().Contain("urn:message:fcg:order-placed:v1");

        var expected = Samples.ReadObject("order-placed.json");
        var actual = envelope["message"]!.AsObject();

        actual.Select(property => property.Key).Should().BeEquivalentTo(expected.Select(property => property.Key));
        actual["price"]!.GetValueKind().Should().Be(JsonValueKind.Number);
        actual["price"]!.ToJsonString().Should().Be(expected["price"]!.ToJsonString());
        actual["orderId"]!.GetValue<Guid>().Should().Be(order.OrderId);
        actual["userId"]!.GetValue<Guid>().Should().Be(userId);
        actual["gameId"]!.GetValue<Guid>().Should().Be(game.Id);
        actual["eventId"]!.GetValue<Guid>().Should().NotBeEmpty();
        actual["occurredAt"]!.GetValueKind().Should().Be(JsonValueKind.String);
    }

    [Fact]
    public async Task PaymentProcessed_DeveConsumirStatusEmTexto_ComoPublicadoPelaPayments()
    {
        var userId = Guid.CreateVersion7();
        var game = await _factory.CreateAdminClient().CreateGameAsync(199.90m);
        var order = await (await _factory.CreateUserClient(userId).PostAsJsonAsync("/api/v1/orders", new { gameId = game.Id }))
            .ReadAsAsync<PlaceOrderResponse>();

        // Sample do contrato ("status": "Approved", "price": 199.90) com os ids deste pedido.
        var sample = Samples.ReadObject("payment-processed.json");
        sample["status"]!.GetValueKind().Should().Be(JsonValueKind.String);
        sample["orderId"] = order.OrderId.ToString();
        sample["userId"] = userId.ToString();
        sample["gameId"] = game.Id.ToString();
        var message = JsonSerializer.Deserialize<PaymentProcessedEvent>(sample.ToJsonString(), SystemTextJsonMessageSerializer.Options)!;

        await _factory.Harness.Bus.Publish(message);

        (await Eventually.TrueAsync(async () => (await _factory.QueryDbAsync(context => context.Set<OrderPersistenceModel>()
            .AsNoTracking().SingleAsync(item => item.Id == order.OrderId))).Status == OrderStatus.Completed)).Should().BeTrue();

        var received = _factory.Harness.Consumed.Select<PaymentProcessedEvent>(context => context.Context.Message.OrderId == order.OrderId).First();
        var payload = JsonNode.Parse(received.Context.ReceiveContext.Body.GetString())!["message"]!.AsObject();
        payload["status"]!.GetValue<string>().Should().Be("Approved");
        payload["price"]!.GetValueKind().Should().Be(JsonValueKind.Number);
        received.Context.ReceiveContext.InputAddress.AbsolutePath.Trim('/').Should().Be(QueueNames.CatalogPaymentProcessed);
    }
}
