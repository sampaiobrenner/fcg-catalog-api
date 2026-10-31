using Fcg.Catalog.Application._Shared.Parsers;
using Fcg.Catalog.Domain.Promotions;

namespace Fcg.Catalog.Application.Promotions;

internal sealed class PromotionResponseParser : Parser<Promotion, PromotionResponse>
{
    protected override PromotionResponse DoParse(Promotion source) => new(
        source.Id(),
        source.Name,
        source.DiscountPercent,
        source.StartsAt,
        source.EndsAt,
        source.Active,
        source.GameIds);
}
