using Fcg.Catalog.Domain._Shared.Models;

namespace Fcg.Catalog.Domain.Promotions;

/// <summary>Associacao entre uma promocao e um jogo participante.</summary>
public sealed class PromotionGamePersistenceModel : PersistenceModelBase
{
    public Guid PromotionId { get; set; }
    public Guid GameId { get; set; }
}
