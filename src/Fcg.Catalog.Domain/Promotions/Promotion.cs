using Fcg.Catalog.Domain._Shared.Exceptions;
using Fcg.Catalog.Domain._Shared.Models;
using Fcg.Catalog.Domain.Properties;

namespace Fcg.Catalog.Domain.Promotions;

/// <summary>Raiz de agregado: promocao de desconto aplicavel a um conjunto de jogos.</summary>
public sealed class Promotion : DomainModelBase<PromotionPersistenceModel>, IAggregateRoot
{
    public Promotion(PromotionPersistenceModel persistenceModel) : base(persistenceModel)
    {
    }

    public string Name => PersistenceModel.Name;

    public decimal DiscountPercent => PersistenceModel.DiscountPercent;

    public DateTime StartsAt => PersistenceModel.StartsAt;

    public DateTime EndsAt => PersistenceModel.EndsAt;

    public bool Active => PersistenceModel.Active;

    public DateTime CreatedAt => PersistenceModel.CreatedAt;

    public IReadOnlyCollection<Guid> GameIds => PersistenceModel.Games.Select(game => game.GameId).ToList();

    public static Promotion Create(string name, decimal discountPercent, DateTime startsAt, DateTime endsAt, IEnumerable<Guid> gameIds)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException(DomainResources.NomeDaPromocaoObrigatorio);

        if (discountPercent is <= 0 or > 100)
            throw new BusinessException(DomainResources.DescontoInvalido);

        var startsAtUtc = ToUtc(startsAt);
        var endsAtUtc = ToUtc(endsAt);

        if (endsAtUtc <= startsAtUtc)
            throw new BusinessException(DomainResources.DataDeTerminoInvalida);

        var model = new PromotionPersistenceModel
        {
            Name = name.Trim(),
            DiscountPercent = discountPercent,
            StartsAt = startsAtUtc,
            EndsAt = endsAtUtc,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var gameId in (gameIds ?? []).Where(id => id != Guid.Empty).Distinct())
            model.Games.Add(new PromotionGamePersistenceModel { PromotionId = model.Id, GameId = gameId });

        if (model.Games.Count == 0)
            throw new BusinessException(DomainResources.PromocaoSemJogos);

        return new Promotion(model);
    }

    public bool AppliesTo(Guid gameId) => PersistenceModel.Games.Exists(game => game.GameId == gameId);

    public bool IsActiveOn(DateTime moment)
    {
        var utc = ToUtc(moment);
        return Active && utc >= StartsAt && utc <= EndsAt;
    }

    public void Deactivate() => PersistenceModel.Active = false;

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
