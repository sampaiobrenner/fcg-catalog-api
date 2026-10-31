using Fcg.Catalog.Domain.Library;
using Fcg.Catalog.Infrastructure._Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Infrastructure.Library;

internal sealed class LibraryRepository : ILibraryRepository
{
    private readonly CatalogDbContext _context;

    public LibraryRepository(CatalogDbContext context) => _context = context;

    public Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken)
        => _context.Set<LibraryItemPersistenceModel>()
            .AnyAsync(item => item.UserId == userId && item.GameId == gameId, cancellationToken);

    public async Task AddAsync(LibraryItem item, CancellationToken cancellationToken)
        => await _context.Set<LibraryItemPersistenceModel>().AddAsync(item.PersistenceModel, cancellationToken);
}
