using MediatR;

namespace Fcg.Catalog.Application.Promotions.CreatePromotion;

public sealed record CreatePromotionCommand(
    string Name,
    decimal DiscountPercent,
    DateTime StartsAt,
    DateTime EndsAt,
    IReadOnlyCollection<Guid> GameIds) : IRequest<PromotionResponse>;
