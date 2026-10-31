using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Domain.Promotions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Application.Promotions.ListPromotions;

internal sealed class ListPromotionsQueryHandler : IRequestHandler<ListPromotionsQuery, IReadOnlyCollection<PromotionResponse>>
{
    private readonly ICatalogDbContext _context;

    public ListPromotionsQueryHandler(ICatalogDbContext context) => _context = context;

    public async Task<IReadOnlyCollection<PromotionResponse>> Handle(ListPromotionsQuery request, CancellationToken cancellationToken)
        => await _context.DataSet<PromotionPersistenceModel>()
            .OrderByDescending(promotion => promotion.CreatedAt)
            .Select(promotion => new PromotionResponse(
                promotion.Id,
                promotion.Name,
                promotion.DiscountPercent,
                promotion.StartsAt,
                promotion.EndsAt,
                promotion.Active,
                promotion.Games.Select(game => game.GameId).ToList()))
            .ToListAsync(cancellationToken);
}
