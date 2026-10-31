using Fcg.Catalog.Application._Shared.Security;
using Fcg.Catalog.Application.Orders.ListMyOrders;
using Fcg.Catalog.Domain.Orders;
using Fcg.Catalog.UnitTests._Fixtures;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Orders.ListMyOrders;

public class ListMyOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_DeveListarSomentePedidosDoUsuario_MaisRecentesPrimeiro()
    {
        await using var context = InMemoryCatalogDbContext.Create();
        var userId = Guid.CreateVersion7();
        var older = new OrderPersistenceModel { UserId = userId, GameId = Guid.CreateVersion7(), Price = 10m, Status = OrderStatus.Completed, CreatedAt = DateTime.UtcNow.AddHours(-2), UpdatedAt = DateTime.UtcNow };
        var newer = new OrderPersistenceModel { UserId = userId, GameId = Guid.CreateVersion7(), Price = 20m, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var other = new OrderPersistenceModel { UserId = Guid.CreateVersion7(), GameId = Guid.CreateVersion7(), Price = 30m, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.AddRange(older, newer, other);
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.UserId).Returns(userId);

        var result = await new ListMyOrdersQueryHandler(context, currentUser.Object).Handle(new ListMyOrdersQuery(), CancellationToken.None);

        result.Select(order => order.OrderId).Should().Equal(newer.Id, older.Id);
    }
}
