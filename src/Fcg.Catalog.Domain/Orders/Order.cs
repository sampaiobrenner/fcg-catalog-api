using Fcg.Catalog.Domain._Shared.Exceptions;
using Fcg.Catalog.Domain._Shared.Models;
using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Library;
using Fcg.Catalog.Domain.Properties;

namespace Fcg.Catalog.Domain.Orders;

/// <summary>
/// Raiz de agregado: pedido de compra de um jogo. Nasce <see cref="OrderStatus.Pending"/> e so muda
/// de situacao uma vez, para <see cref="OrderStatus.Completed"/> ou <see cref="OrderStatus.Rejected"/>.
/// </summary>
public sealed class Order : DomainModelBase<OrderPersistenceModel>, IAggregateRoot
{
    public const int ReasonMaxLength = 300;

    public Order(OrderPersistenceModel persistenceModel) : base(persistenceModel)
    {
    }

    public Guid UserId => PersistenceModel.UserId;

    public Guid GameId => PersistenceModel.GameId;

    public Money Price => Money.Create(PersistenceModel.Price);

    public OrderStatus Status => PersistenceModel.Status;

    public string? Reason => PersistenceModel.Reason;

    public DateTime CreatedAt => PersistenceModel.CreatedAt;

    public DateTime UpdatedAt => PersistenceModel.UpdatedAt;

    public static Order Place(Guid userId, Guid gameId, Money price)
    {
        if (userId == Guid.Empty)
            throw new BusinessException(string.Format(DomainResources.CampoObrigatorio, nameof(UserId)));

        if (gameId == Guid.Empty)
            throw new BusinessException(string.Format(DomainResources.CampoObrigatorio, nameof(GameId)));

        ArgumentNullException.ThrowIfNull(price);

        var now = DateTime.UtcNow;

        return new Order(new OrderPersistenceModel
        {
            UserId = userId,
            GameId = gameId,
            Price = price.Amount,
            Status = OrderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    /// <summary>Conclui o pedido aprovado e devolve o item de biblioteca correspondente.</summary>
    public LibraryItem Complete()
    {
        EnsurePending();

        var now = DateTime.UtcNow;
        PersistenceModel.Status = OrderStatus.Completed;
        PersistenceModel.Reason = null;
        PersistenceModel.UpdatedAt = now;

        return LibraryItem.Acquire(UserId, GameId, Price, now);
    }

    public void Reject(string? reason)
    {
        EnsurePending();

        var trimmed = string.IsNullOrWhiteSpace(reason) ? DomainResources.PagamentoRecusadoSemMotivo : reason.Trim();

        PersistenceModel.Status = OrderStatus.Rejected;
        PersistenceModel.Reason = trimmed.Length > ReasonMaxLength ? trimmed[..ReasonMaxLength] : trimmed;
        PersistenceModel.UpdatedAt = DateTime.UtcNow;
    }

    private void EnsurePending()
    {
        if (Status != OrderStatus.Pending)
            throw new BusinessException(string.Format(DomainResources.TransicaoDePedidoInvalida, Status));
    }
}
