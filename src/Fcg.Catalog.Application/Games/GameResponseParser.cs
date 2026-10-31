using Fcg.Catalog.Application._Shared.Parsers;
using Fcg.Catalog.Domain.Games;

namespace Fcg.Catalog.Application.Games;

internal sealed class GameResponseParser : Parser<Game, GameResponse>
{
    protected override GameResponse DoParse(Game source) => new(
        source.Id(),
        source.Title,
        source.Description,
        source.Genre,
        source.Price.Amount,
        source.ReleaseDate,
        source.Active,
        source.CreatedAt);
}
