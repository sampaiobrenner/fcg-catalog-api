using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Orders.CompletePurchase;
using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Library;
using Fcg.Catalog.Domain.Orders;
using Fcg.Contracts.Events.V1;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Orders.CompletePurchase;

public class CompletePurchaseCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<ILibraryRepository> _library = new();
    private readonly Mock<ICatalogDbContext> _context = new();
    private readonly Order _order = Order.Place(Guid.CreateVersion7(), Guid.CreateVersion7(), Money.Create(59.90m));

    public CompletePurchaseCommandHandlerTests()
        => _orders.Setup(r => r.GetByIdAsync(_order.Id(), It.IsAny<CancellationToken>())).ReturnsAsync(_order);

    private CompletePurchaseCommandHandler CreateSut()
        => new(_orders.Object, _library.Object, _context.Object, NullLogger<CompletePurchaseCommandHandler>.Instance);

    private CompletePurchaseCommand Command(PaymentStatus status, string? reason = null)
        => new(_order.Id(), _order.UserId, _order.GameId, 59.90m, status, reason);

    [Fact]
    public async Task Handle_DeveConcluirPedidoEAdicionarNaBiblioteca_QuandoAprovado()
    {
        LibraryItem? added = null;
        _library.Setup(r => r.AddAsync(It.IsAny<LibraryItem>(), It.IsAny<CancellationToken>()))
            .Callback<LibraryItem, CancellationToken>((item, _) => added = item);

        await CreateSut().Handle(Command(PaymentStatus.Approved), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Completed);
        added.Should().NotBeNull();
        added!.UserId.Should().Be(_order.UserId);
        added.GameId.Should().Be(_order.GameId);
        added.PricePaid.Amount.Should().Be(59.90m);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRecusarPedidoComMotivo_QuandoRejeitado()
    {
        await CreateSut().Handle(Command(PaymentStatus.Rejected, "Valor acima do limite de aprovacao"), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Rejected);
        _order.Reason.Should().Be("Valor acima do limite de aprovacao");
        _library.Verify(r => r.AddAsync(It.IsAny<LibraryItem>(), It.IsAny<CancellationToken>()), Times.Never);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(PaymentStatus.Approved)]
    [InlineData(PaymentStatus.Rejected)]
    public async Task Handle_DeveIgnorar_QuandoPedidoJaConcluido(PaymentStatus status)
    {
        _order.Complete();

        await CreateSut().Handle(Command(status, "tarde demais"), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Completed);
        _order.Reason.Should().BeNull();
        _library.Verify(r => r.AddAsync(It.IsAny<LibraryItem>(), It.IsAny<CancellationToken>()), Times.Never);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NaoDeveDuplicarBiblioteca_QuandoItemJaExiste()
    {
        _library.Setup(r => r.ExistsAsync(_order.UserId, _order.GameId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await CreateSut().Handle(Command(PaymentStatus.Approved), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Completed);
        _library.Verify(r => r.AddAsync(It.IsAny<LibraryItem>(), It.IsAny<CancellationToken>()), Times.Never);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoPedidoInexistente()
    {
        var act = () => CreateSut().Handle(
            new CompletePurchaseCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), 10m, PaymentStatus.Approved, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public void Validator_DeveRejeitar_QuandoOrderIdVazioOuStatusInvalido()
    {
        var result = new CompletePurchaseCommandValidator().Validate(
            new CompletePurchaseCommand(Guid.Empty, Guid.CreateVersion7(), Guid.CreateVersion7(), 10m, (PaymentStatus)99, null));

        result.Errors.Select(error => error.PropertyName).Should().Contain([nameof(CompletePurchaseCommand.OrderId), nameof(CompletePurchaseCommand.Status)]);
    }
}
