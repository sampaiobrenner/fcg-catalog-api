using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Promotions;

namespace Fcg.Catalog.UnitTests._Fixtures;

internal static class TestData
{
    public static Game Game(decimal price = 100m, string title = "Portal 2", string genre = "Puzzle")
        => Fcg.Catalog.Domain.Games.Game.Create(title, "Puzzle cooperativo", genre, Money.Create(price), new DateOnly(2011, 4, 18));

    public static Promotion ActivePromotion(Guid gameId, decimal discount = 20m, string name = "Promocao")
        => Promotion.Create(name, discount, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), [gameId]);
}
