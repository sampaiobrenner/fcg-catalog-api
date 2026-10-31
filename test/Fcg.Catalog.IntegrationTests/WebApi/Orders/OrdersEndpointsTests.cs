using System.Net;
using System.Net.Http.Json;
using Fcg.Catalog.Application.Orders;
using Fcg.Catalog.Application.Orders.PlaceOrder;
using Fcg.Catalog.Domain.Library;
using Fcg.Catalog.Domain.Orders;
using Fcg.Catalog.IntegrationTests._Fixtures;
using Fcg.Contracts.Events.V1;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.IntegrationTests.WebApi.Orders;

[Collection(CatalogApiCollection.Name)]
public class OrdersEndpointsTests
{
    private readonly CatalogApiFactory _factory;
    private readonly HttpClient _admin;

    public OrdersEndpointsTests(CatalogApiFactory factory)
    {
        _factory = factory;
        _admin = factory.CreateAdminClient();
    }

    [Fact]
    public async Task Place_DeveRetornar202ComPrecoPromocionalEPublicarEvento()
    {
        var userId = Guid.CreateVersion7();
        var game = await _admin.CreateGameAsync(199.90m);
        await _admin.CreatePromotionAsync(10m, game.Id);

        var response = await _factory.CreateUserClient(userId).PostAsJsonAsync("/api/v1/orders", new { gameId = game.Id });

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var body = await response.ReadAsAsync<PlaceOrderResponse>();
        body.Status.Should().Be(OrderStatus.Pending);
        body.Price.Should().Be(179.91m);
        response.Headers.Location!.ToString().Should().Be($"/api/v1/orders/{body.OrderId}");
        (await response.Content.ReadAsStringAsync()).Should().Contain("\"status\":\"Pending\"");

        // Polling em vez de Published.Any: o outbox entrega de forma assincrona e o harness e compartilhado entre testes.
        (await Eventually.TrueAsync(() => Task.FromResult(_factory.Harness.Published
            .Select<OrderPlacedEvent>(context => context.Context.Message.OrderId == body.OrderId).Any()))).Should().BeTrue();
        var published = _factory.Harness.Published.Select<OrderPlacedEvent>()
            .Single(context => context.Context.Message.OrderId == body.OrderId).Context.Message;
        published.UserId.Should().Be(userId);
        published.GameId.Should().Be(game.Id);
        published.Price.Should().Be(179.91m);

        var stored = await _factory.QueryDbAsync(context => context.Set<OrderPersistenceModel>().AsNoTracking().SingleAsync(order => order.Id == body.OrderId));
        stored.Status.Should().Be(OrderStatus.Pending);
        stored.Price.Should().Be(179.91m);
    }

    [Fact]
    public async Task Place_DeveRetornar404_QuandoJogoInexistente()
    {
        var response = await _factory.CreateUserClient(Guid.CreateVersion7()).PostAsJsonAsync("/api/v1/orders", new { gameId = Guid.CreateVersion7() });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Place_DeveRetornar404_QuandoJogoInativo()
    {
        var game = await _admin.CreateGameAsync(10m);
        await _admin.DeleteAsync($"/api/v1/games/{game.Id}");

        var response = await _factory.CreateUserClient(Guid.CreateVersion7()).PostAsJsonAsync("/api/v1/orders", new { gameId = game.Id });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Place_DeveRetornar409_QuandoJaExistePedidoPendente()
    {
        var client = _factory.CreateUserClient(Guid.CreateVersion7());
        var game = await _admin.CreateGameAsync(10m);
        (await client.PostAsJsonAsync("/api/v1/orders", new { gameId = game.Id })).StatusCode.Should().Be(HttpStatusCode.Accepted);

        var response = await client.PostAsJsonAsync("/api/v1/orders", new { gameId = game.Id });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain("pedido em andamento");
    }

    [Fact]
    public async Task Place_DeveRetornar409_QuandoJogoJaNaBiblioteca()
    {
        var userId = Guid.CreateVersion7();
        var game = await _admin.CreateGameAsync(10m);
        await _factory.ExecuteDbAsync(async context =>
        {
            context.Add(new LibraryItemPersistenceModel { UserId = userId, GameId = game.Id, PricePaid = 10m, AcquiredAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
        });

        var response = await _factory.CreateUserClient(userId).PostAsJsonAsync("/api/v1/orders", new { gameId = game.Id });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain("biblioteca");
    }

    [Fact]
    public async Task Place_DeveRetornar400_QuandoGameIdVazio()
    {
        var response = await _factory.CreateUserClient(Guid.CreateVersion7()).PostAsJsonAsync("/api/v1/orders", new { gameId = Guid.Empty });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Place_DeveRetornar401_QuandoAnonimo()
    {
        var response = await _factory.CreateAnonymousClient().PostAsJsonAsync("/api/v1/orders", new { gameId = Guid.CreateVersion7() });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_DeveRespeitarDonoEAdministrador()
    {
        var ownerId = Guid.CreateVersion7();
        var owner = _factory.CreateUserClient(ownerId);
        var game = await _admin.CreateGameAsync(35m);
        var placed = await (await owner.PostAsJsonAsync("/api/v1/orders", new { gameId = game.Id })).ReadAsAsync<PlaceOrderResponse>();

        var ownerResponse = await owner.GetAsync($"/api/v1/orders/{placed.OrderId}");
        ownerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await ownerResponse.ReadAsAsync<OrderResponse>();
        order.UserId.Should().Be(ownerId);
        order.GameId.Should().Be(game.Id);
        order.Price.Should().Be(35m);
        order.Status.Should().Be(OrderStatus.Pending);

        (await _factory.CreateUserClient(Guid.CreateVersion7()).GetAsync($"/api/v1/orders/{placed.OrderId}"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);

        (await _admin.GetAsync($"/api/v1/orders/{placed.OrderId}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task List_DeveRetornarPedidosDoUsuario()
    {
        var client = _factory.CreateUserClient(Guid.CreateVersion7());
        var first = await _admin.CreateGameAsync(10m);
        var second = await _admin.CreateGameAsync(20m);
        await client.PostAsJsonAsync("/api/v1/orders", new { gameId = first.Id });
        await client.PostAsJsonAsync("/api/v1/orders", new { gameId = second.Id });

        var orders = await (await client.GetAsync("/api/v1/orders")).ReadAsAsync<List<OrderResponse>>();

        orders.Select(order => order.GameId).Should().Equal(second.Id, first.Id);
    }
}
