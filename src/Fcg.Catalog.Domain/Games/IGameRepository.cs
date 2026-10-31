namespace Fcg.Catalog.Domain.Games;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);

    Task<bool> TitleExistsAsync(string title, CancellationToken cancellationToken);

    Task AddAsync(Game game, CancellationToken cancellationToken);
}
