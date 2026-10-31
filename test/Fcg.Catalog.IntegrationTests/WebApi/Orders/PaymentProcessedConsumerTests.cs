using System.Net.Http.Json;
using Fcg.Catalog.Application.Library.GetMyLibrary;
using Fcg.Catalog.Application.Orders;
using Fcg.Catalog.Application.Orders.PlaceOrder;
using Fcg.Catalog.Domain.Library;
using Fcg.Catalog.Domain.Orders;
using Fcg.Catalog.IntegrationTests._Fixtures;
using Fcg.Catalog.WebApi.Orders.CompletePurchase;
using Fcg.Contracts.Events.V1;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.IntegrationTests.WebApi.Orders;

[Collection(CatalogApiCollection.Name)]
public class PaymentProcessedConsumerTests
{
    private readonly CatalogApiFactory _factory;

    public PaymentProcessedConsumerTests(CatalogApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Approved_DeveAdicionarJogoNaBibliotecaEConcluirPedido()
    {
        var (userId, client, order) = await PlaceOrderAsync(45m);

        await PublishAsync(order, userId, PaymentStatus.Approved);

        (await Eventually.TrueAsync(async () => (await GetOrderAsync(order.OrderId)).Status == OrderStatus.Completed)).Should().BeTrue();

        var library = await (await client.GetAsync("/api/v1/library")).ReadAsAsync<List<LibraryItemResponse>>();
        var item = library.Should().ContainSingle().Subject;
        item.PricePaid.Should().Be(45m);

        var detail = await (await client.GetAsync($"/api/v1/orders/{order.OrderId}")).ReadAsAsync<OrderResponse>();
        detail.Status.Should().Be(OrderStatus.Completed);
        detail.Reason.Should().BeNull();
    }

    [Fact]
    public async Task Rejected_DeveRecusarPedidoComMotivo()
    {
        var (userId, client, order) = await PlaceOrderAsync(450m);

        await PublishAsync(order, userId, PaymentStatus.Rejected, "Valor acima do limite de aprovacao");

        (await Eventually.TrueAsync(async () => (await GetOrderAsync(order.OrderId)).Status == OrderStatus.Rejected)).Should().BeTrue();

        var stored = await GetOrderAsync(order.OrderId);
        stored.Reason.Should().Be("Valor acima do limite de aprovacao");
        (await (await client.GetAsync("/api/v1/library")).ReadAsAsync<List<LibraryItemResponse>>()).Should().BeEmpty();
    }

    [Fact]
    public async Task EventoDuplicado_NaoDeveDuplicarBiblioteca()
    {
        var (userId, _, order) = await PlaceOrderAsync(30m);

        // Mesmo evento publicado duas vezes: cada publicacao recebe um MessageId diferente (o inbox nao barra).
        var message = Event(order, userId, PaymentStatus.Approved);
        await _factory.Harness.Bus.Publish(message);
        await _factory.Harness.Bus.Publish(message);

        (await Eventually.TrueAsync(async () => await ConsumedCountAsync(order.OrderId) >= 2)).Should().BeTrue();
        (await GetOrderAsync(order.OrderId)).Status.Should().Be(OrderStatus.Completed);
        (await LibraryCountAsync(userId)).Should().Be(1);
    }

    [Fact]
    public async Task PedidoJaConcluido_DeveIgnorarNovoResultado()
    {
        var (userId, _, order) = await PlaceOrderAsync(20m);
        await PublishAsync(order, userId, PaymentStatus.Approved);
        (await Eventually.TrueAsync(async () => (await GetOrderAsync(order.OrderId)).Status == OrderStatus.Completed)).Should().BeTrue();

        await PublishAsync(order, userId, PaymentStatus.Rejected, "tarde demais");

        (await Eventually.TrueAsync(async () => await ConsumedCountAsync(order.OrderId) >= 2)).Should().BeTrue();
        var stored = await GetOrderAsync(order.OrderId);
        stored.Status.Should().Be(OrderStatus.Completed);
        stored.Reason.Should().BeNull();
        (await LibraryCountAsync(userId)).Should().Be(1);
    }

    [Fact]
    public void Consumer_DeveEstarRegistradoNaFilaDoContrato()
    {
        _factory.Harness.GetConsumerHarness<PaymentProcessedConsumer>().Should().NotBeNull();
    }

    private async Task<(Guid UserId, HttpClient Client, PlaceOrderResponse Order)> PlaceOrderAsync(decimal price)
    {
        var userId = Guid.CreateVersion7();
        var client = _factory.CreateUserClient(userId);
        var game = await _factory.CreateAdminClient().CreateGameAsync(price);
        var order = await (await client.PostAsJsonAsync("/api/v1/orders", new { gameId = game.Id })).ReadAsAsync<PlaceOrderResponse>();
        return (userId, client, order);
    }

    private static PaymentProcessedEvent Event(PlaceOrderResponse order, Guid userId, PaymentStatus status, string? reason = null)
        => new(Guid.CreateVersion7(), DateTime.UtcNow, order.OrderId, userId, Guid.Empty, order.Price, status, reason);

    private async Task PublishAsync(PlaceOrderResponse order, Guid userId, PaymentStatus status, string? reason = null)
    {
        var stored = await GetOrderAsync(order.OrderId);
        await _factory.Harness.Bus.Publish(Event(order, userId, status, reason) with { GameId = stored.GameId });
    }

    private Task<OrderPersistenceModel> GetOrderAsync(Guid orderId)
        => _factory.QueryDbAsync(context => context.Set<OrderPersistenceModel>().AsNoTracking().SingleAsync(order => order.Id == orderId));

    private Task<int> LibraryCountAsync(Guid userId)
        => _factory.QueryDbAsync(context => context.Set<LibraryItemPersistenceModel>().CountAsync(item => item.UserId == userId));

    private Task<int> ConsumedCountAsync(Guid orderId)
        => Task.FromResult(_factory.Harness.Consumed
            .Select<PaymentProcessedEvent>(context => context.Context.Message.OrderId == orderId)
            .Count(context => context.Exception is null));
}
