using Fcg.Catalog.Domain.Promotions;
using Fcg.Catalog.Infrastructure._Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Infrastructure.Promotions;

internal sealed class PromotionRepository : IPromotionRepository
{
    private readonly CatalogDbContext _context;

    public PromotionRepository(CatalogDbContext context) => _context = context;

    public async Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await _context.Set<PromotionPersistenceModel>()
            .Include(promotion => promotion.Games)
            .FirstOrDefaultAsync(promotion => promotion.Id == id, cancellationToken);

        return model is null ? null : new Promotion(model);
    }

    public async Task<IReadOnlyCollection<Promotion>> GetActiveForGameAsync(Guid gameId, DateTime moment, CancellationToken cancellationToken)
    {
        var models = await _context.Set<PromotionPersistenceModel>()
            .AsNoTracking()
            .Include(promotion => promotion.Games)
            .Where(promotion => promotion.Active
                                && promotion.StartsAt <= moment
                                && promotion.EndsAt >= moment
                                && promotion.Games.Any(game => game.GameId == gameId))
            .ToListAsync(cancellationToken);

        return models.Select(model => new Promotion(model)).ToList();
    }

    public async Task AddAsync(Promotion promotion, CancellationToken cancellationToken)
        => await _context.Set<PromotionPersistenceModel>().AddAsync(promotion.PersistenceModel, cancellationToken);
}
