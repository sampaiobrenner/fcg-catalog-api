namespace Fcg.Catalog.Domain.Promotions;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Promocoes ativas no momento informado que incluem o jogo.</summary>
    Task<IReadOnlyCollection<Promotion>> GetActiveForGameAsync(Guid gameId, DateTime moment, CancellationToken cancellationToken);

    Task AddAsync(Promotion promotion, CancellationToken cancellationToken);
}
