using Fcg.Catalog.Domain._Shared.Exceptions;
using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Orders;

namespace Fcg.Catalog.UnitTests.Domain.Orders;

public class OrderTests
{
    private static Order NewOrder(decimal price = 59.93m)
        => Order.Place(Guid.CreateVersion7(), Guid.CreateVersion7(), Money.Create(price));

    [Fact]
    public void Place_DeveCriarPedidoPendente()
    {
        var userId = Guid.CreateVersion7();
        var gameId = Guid.CreateVersion7();

        var order = Order.Place(userId, gameId, Money.Create(199.90m));

        order.Id().Should().NotBeEmpty();
        order.UserId.Should().Be(userId);
        order.GameId.Should().Be(gameId);
        order.Price.Amount.Should().Be(199.90m);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Reason.Should().BeNull();
        order.UpdatedAt.Should().Be(order.CreatedAt);
    }

    [Fact]
    public void Place_DeveLancarExcecao_QuandoUsuarioOuJogoVazio()
    {
        var semUsuario = () => Order.Place(Guid.Empty, Guid.CreateVersion7(), Money.Create(10m));
        var semJogo = () => Order.Place(Guid.CreateVersion7(), Guid.Empty, Money.Create(10m));

        semUsuario.Should().Throw<BusinessException>();
        semJogo.Should().Throw<BusinessException>();
    }

    [Fact]
    public void Complete_DeveConcluirPedidoEGerarItemDaBiblioteca()
    {
        var order = NewOrder();

        var item = order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
        order.UpdatedAt.Should().BeOnOrAfter(order.CreatedAt);
        item.UserId.Should().Be(order.UserId);
        item.GameId.Should().Be(order.GameId);
        item.PricePaid.Amount.Should().Be(59.93m);
        item.AcquiredAt.Should().Be(order.UpdatedAt);
    }

    [Fact]
    public void Reject_DeveRecusarPedidoComMotivo()
    {
        var order = NewOrder();

        order.Reject("  Valor acima do limite  ");

        order.Status.Should().Be(OrderStatus.Rejected);
        order.Reason.Should().Be("Valor acima do limite");
    }

    [Fact]
    public void Reject_DeveUsarMotivoPadrao_QuandoMotivoVazio()
    {
        var order = NewOrder();

        order.Reject(null);

        order.Reason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Reject_DeveTruncarMotivo_QuandoMaiorQueOLimite()
    {
        var order = NewOrder();

        order.Reject(new string('x', 400));

        order.Reason.Should().HaveLength(Order.ReasonMaxLength);
    }

    public static TheoryData<Action<Order>, Action<Order>> TransicoesInvalidas => new()
    {
        { order => order.Complete(), order => order.Complete() },
        { order => order.Complete(), order => order.Reject("x") },
        { order => order.Reject("x"), order => order.Complete() },
        { order => order.Reject("x"), order => order.Reject("y") }
    };

    [Theory]
    [MemberData(nameof(TransicoesInvalidas))]
    public void Transicao_DeveLancarExcecao_QuandoPedidoNaoEstaPendente(Action<Order> primeira, Action<Order> segunda)
    {
        var order = NewOrder();
        primeira(order);
        var status = order.Status;

        var act = () => segunda(order);

        act.Should().Throw<BusinessException>().WithMessage("*Pending*");
        order.Status.Should().Be(status);
    }
}
