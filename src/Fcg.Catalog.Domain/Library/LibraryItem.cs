using Fcg.Catalog.Domain._Shared.Models;
using Fcg.Catalog.Domain._Shared.ValueObjects;

namespace Fcg.Catalog.Domain.Library;

/// <summary>
/// Item da biblioteca: um jogo adquirido por um usuario. Nao ha "adquirir" publico:
/// o item so nasce da conclusao de um pedido aprovado (<c>Order.Complete</c>).
/// </summary>
public sealed class LibraryItem : DomainModelBase<LibraryItemPersistenceModel>, IAggregateRoot
{
    private LibraryItem(LibraryItemPersistenceModel persistenceModel) : base(persistenceModel)
    {
    }

    public Guid UserId => PersistenceModel.UserId;

    public Guid GameId => PersistenceModel.GameId;

    public Money PricePaid => Money.Create(PersistenceModel.PricePaid);

    public DateTime AcquiredAt => PersistenceModel.AcquiredAt;

    internal static LibraryItem Acquire(Guid userId, Guid gameId, Money pricePaid, DateTime acquiredAt)
        => new(new LibraryItemPersistenceModel
        {
            UserId = userId,
            GameId = gameId,
            PricePaid = pricePaid.Amount,
            AcquiredAt = acquiredAt
        });
}
