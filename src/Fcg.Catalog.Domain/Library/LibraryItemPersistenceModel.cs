using Fcg.Catalog.Domain._Shared.Models;

namespace Fcg.Catalog.Domain.Library;

public sealed class LibraryItemPersistenceModel : PersistenceModelBase
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public decimal PricePaid { get; set; }
    public DateTime AcquiredAt { get; set; }
}
