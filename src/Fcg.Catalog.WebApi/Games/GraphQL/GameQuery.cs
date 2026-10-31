using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Domain.Games;
using HotChocolate;
using HotChocolate.Data;

namespace Fcg.Catalog.WebApi.Games.GraphQL;

/// <summary>Consulta GraphQL do catalogo (portada do monolito): filtros e ordenacao traduzidos para SQL pelo EF.</summary>
public sealed class GameQuery
{
    [GraphQLDescription("Consulta o catalogo de jogos ativos, permitindo filtragem e ordenacao dinamicas.")]
    [UseFiltering]
    [UseSorting]
    public IQueryable<GameGraphDto> GetGames([Service] ICatalogDbContext context)
        => context.DataSet<GamePersistenceModel>()
            .Where(game => game.Active)
            .Select(game => new GameGraphDto
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Genre = game.Genre,
                Price = game.Price,
                ReleaseDate = game.ReleaseDate
            });
}
