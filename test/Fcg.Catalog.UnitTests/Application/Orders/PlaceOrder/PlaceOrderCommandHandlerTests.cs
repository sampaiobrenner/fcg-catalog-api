using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application._Shared.Messaging;
using Fcg.Catalog.Application._Shared.Security;
using Fcg.Catalog.Application.Orders.PlaceOrder;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Library;
using Fcg.Catalog.Domain.Orders;
using Fcg.Catalog.Domain.Pricing;
using Fcg.Catalog.Domain.Promotions;
using Fcg.Catalog.UnitTests._Fixtures;
using Fcg.Contracts.Events.V1;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Orders.PlaceOrder;

public class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<IPromotionRepository> _promotions = new();
    private readonly Mock<ILibraryRepository> _library = new();
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IIntegrationEventPublisher> _publisher = new();
    private readonly Mock<ICatalogDbContext> _context = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Guid _userId = Guid.CreateVersion7();
    private readonly Game _game = TestData.Game(79.90m);

    public PlaceOrderCommandHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns(_userId);
        _games.Setup(r => r.GetByIdAsync(_game.Id(), It.IsAny<CancellationToken>())).ReturnsAsync(_game);
        _promotions.Setup(r => r.GetActiveForGameAsync(_game.Id(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    }

    private PlaceOrderCommandHandler CreateSut() => new(
        _games.Object,
        _promotions.Object,
        _library.Object,
        _orders.Object,
        new PricingService(),
        _publisher.Object,
        _context.Object,
        _currentUser.Object,
        new PlaceOrderResponseParser());

    [Fact]
    public async Task Handle_DeveCriarPedidoPendenteEPublicarEvento_QuandoJogoDisponivel()
    {
        Order? added = null;
        OrderPlacedEvent? published = null;
        _orders.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => added = order);
        _publisher.Setup(p => p.PublishAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<OrderPlacedEvent, CancellationToken>((message, _) => published = message);

        var response = await CreateSut().Handle(new PlaceOrderCommand(_game.Id()), CancellationToken.None);

        response.Status.Should().Be(OrderStatus.Pending);
        response.Price.Should().Be(79.90m);
        added.Should().NotBeNull();
        response.OrderId.Should().Be(added!.Id());

        published.Should().NotBeNull();
        published!.EventId.Should().NotBeEmpty();
        published.OrderId.Should().Be(response.OrderId);
        published.UserId.Should().Be(_userId);
        published.GameId.Should().Be(_game.Id());
        published.Price.Should().Be(79.90m);
        published.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Handle_DeveAplicarMaiorDescontoAtivo_QuandoHaPromocoes()
    {
        _promotions.Setup(r => r.GetActiveForGameAsync(_game.Id(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([TestData.ActivePromotion(_game.Id(), 10m), TestData.ActivePromotion(_game.Id(), 25m)]);

        var response = await CreateSut().Handle(new PlaceOrderCommand(_game.Id()), CancellationToken.None);

        response.Price.Should().Be(59.93m);
        _publisher.Verify(p => p.PublishAsync(It.Is<OrderPlacedEvent>(e => e.Price == 59.93m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DevePublicarAntesDoSaveChanges()
    {
        var calls = new List<string>();
        _publisher.Setup(p => p.PublishAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("publish"));
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("save"))
            .ReturnsAsync(1);

        await CreateSut().Handle(new PlaceOrderCommand(_game.Id()), CancellationToken.None);

        calls.Should().Equal("publish", "save");
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoJogoInexistente()
    {
        var act = () => CreateSut().Handle(new PlaceOrderCommand(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        VerifyNothingPersisted();
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoJogoInativo()
    {
        _game.Deactivate();

        var act = () => CreateSut().Handle(new PlaceOrderCommand(_game.Id()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        VerifyNothingPersisted();
    }

    [Fact]
    public async Task Handle_DeveLancarConflito_QuandoJogoJaNaBiblioteca()
    {
        _library.Setup(r => r.ExistsAsync(_userId, _game.Id(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => CreateSut().Handle(new PlaceOrderCommand(_game.Id()), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*biblioteca*");
        VerifyNothingPersisted();
    }

    [Fact]
    public async Task Handle_DeveLancarConflito_QuandoJaExistePedidoPendente()
    {
        _orders.Setup(r => r.HasPendingAsync(_userId, _game.Id(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => CreateSut().Handle(new PlaceOrderCommand(_game.Id()), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*pedido em andamento*");
        VerifyNothingPersisted();
    }

    [Fact]
    public async Task Handle_DeveLancarNaoAutenticado_QuandoTokenSemSub()
    {
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = () => CreateSut().Handle(new PlaceOrderCommand(_game.Id()), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public void Validator_DeveExigirGameId()
    {
        new PlaceOrderCommandValidator().Validate(new PlaceOrderCommand(Guid.Empty)).IsValid.Should().BeFalse();
    }

    private void VerifyNothingPersisted()
    {
        _orders.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
