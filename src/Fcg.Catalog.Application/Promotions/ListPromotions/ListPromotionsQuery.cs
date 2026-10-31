using MediatR;

namespace Fcg.Catalog.Application.Promotions.ListPromotions;

public sealed record ListPromotionsQuery : IRequest<IReadOnlyCollection<PromotionResponse>>;
