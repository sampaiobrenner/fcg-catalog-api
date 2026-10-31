namespace Fcg.Catalog.Domain.Library;

public interface ILibraryRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken);

    Task AddAsync(LibraryItem item, CancellationToken cancellationToken);
}
