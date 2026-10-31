using Fcg.Catalog.Domain._Shared.Models;

namespace Fcg.Catalog.Domain.Promotions;

public sealed class PromotionPersistenceModel : PersistenceModelBase
{
    public string Name { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PromotionGamePersistenceModel> Games { get; set; } = [];
}
