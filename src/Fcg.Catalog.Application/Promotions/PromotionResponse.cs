namespace Fcg.Catalog.Application.Promotions;

public sealed record PromotionResponse(
    Guid Id,
    string Name,
    decimal DiscountPercent,
    DateTime StartsAt,
    DateTime EndsAt,
    bool Active,
    IReadOnlyCollection<Guid> GameIds);
