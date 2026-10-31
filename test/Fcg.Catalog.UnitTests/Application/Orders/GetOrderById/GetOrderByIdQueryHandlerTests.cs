using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application._Shared.Security;
using Fcg.Catalog.Application.Orders.GetOrderById;
using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Orders;
using Fcg.Catalog.Infrastructure._Shared.Context;
using Fcg.Catalog.UnitTests._Fixtures;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Orders.GetOrderById;

public class GetOrderByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly CatalogDbContext _context = InMemoryCatalogDbContext.Create();
    private readonly Order _order = Order.Place(Guid.CreateVersion7(), Guid.CreateVersion7(), Money.Create(49.90m));

    public async Task InitializeAsync()
    {
        _context.Add(_order.PersistenceModel);
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    private Task<Fcg.Catalog.Application.Orders.OrderResponse> Query(Guid userId, bool administrator, Guid? orderId = null)
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.UserId).Returns(userId);
        currentUser.Setup(u => u.IsAdministrator).Returns(administrator);

        return new GetOrderByIdQueryHandler(_context, currentUser.Object).Handle(new GetOrderByIdQuery(orderId ?? _order.Id()), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_DeveRetornarPedido_QuandoDono()
    {
        var response = await Query(_order.UserId, administrator: false);

        response.OrderId.Should().Be(_order.Id());
        response.UserId.Should().Be(_order.UserId);
        response.GameId.Should().Be(_order.GameId);
        response.Price.Should().Be(49.90m);
        response.Status.Should().Be(OrderStatus.Pending);
        response.Reason.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeveRetornarPedido_QuandoAdministrador()
    {
        (await Query(Guid.CreateVersion7(), administrator: true)).OrderId.Should().Be(_order.Id());
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoOutroUsuario()
    {
        var act = () => Query(Guid.CreateVersion7(), administrator: false);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoPedidoInexistente()
    {
        var act = () => Query(_order.UserId, administrator: true, Guid.CreateVersion7());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
