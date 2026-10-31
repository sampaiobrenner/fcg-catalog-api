using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Infrastructure._Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Infrastructure.Games;

internal sealed class GameRepository : IGameRepository
{
    private readonly CatalogDbContext _context;

    public GameRepository(CatalogDbContext context) => _context = context;

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await _context.Set<GamePersistenceModel>().FirstOrDefaultAsync(game => game.Id == id, cancellationToken);
        return model is null ? null : new Game(model);
    }

    public async Task<IReadOnlyCollection<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.Distinct().ToList();

        return await _context.Set<GamePersistenceModel>()
            .AsNoTracking()
            .Where(game => idList.Contains(game.Id))
            .Select(game => game.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> TitleExistsAsync(string title, CancellationToken cancellationToken)
    {
        var normalized = title.Trim().ToLower();
        return _context.Set<GamePersistenceModel>().AnyAsync(game => game.Title.ToLower() == normalized, cancellationToken);
    }

    public async Task AddAsync(Game game, CancellationToken cancellationToken)
        => await _context.Set<GamePersistenceModel>().AddAsync(game.PersistenceModel, cancellationToken);
}
