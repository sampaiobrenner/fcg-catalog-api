using Dapper;
using Fcg.Catalog.Application._Shared.ReadQueries;
using Fcg.Catalog.Application.Games.SearchGames;
using Fcg.Catalog.Application.Library.GetMyLibrary;
using Fcg.Catalog.Infrastructure._Shared.Dapper;

namespace Fcg.Catalog.Infrastructure.Games;

/// <summary>
/// Consultas de leitura com Dapper (portadas do monolito): catalogo com o preco efetivo
/// da promocao ativa de maior desconto e biblioteca do usuario.
/// </summary>
internal sealed class GameReadQueries : IGameReadQueries
{
    private const string SearchCatalogSql = """
        SELECT  g.id                              AS Id,
                g.title                           AS Title,
                g.description                     AS Description,
                g.genre                           AS Genre,
                g.price                           AS Price,
                promo.discount_percent            AS DiscountPercent,
                CASE WHEN promo.discount_percent IS NOT NULL
                     THEN ROUND(g.price * (1 - promo.discount_percent / 100.0), 2)
                     ELSE g.price END             AS EffectivePrice,
                g.release_date                    AS ReleaseDate
        FROM games g
        LEFT JOIN LATERAL (
            SELECT pr.discount_percent
            FROM promotions pr
            JOIN promotion_games pg ON pg.promotion_id = pr.id
            WHERE pg.game_id = g.id
              AND pr.active = TRUE
              AND now() BETWEEN pr.starts_at AND pr.ends_at
            ORDER BY pr.discount_percent DESC
            LIMIT 1
        ) promo ON TRUE
        WHERE g.active = TRUE
          AND (@Title::text    IS NULL OR g.title ILIKE '%' || @Title::text || '%')
          AND (@Genre::text    IS NULL OR g.genre = @Genre::text)
          AND (@MinPrice::numeric IS NULL OR g.price >= @MinPrice::numeric)
          AND (@MaxPrice::numeric IS NULL OR g.price <= @MaxPrice::numeric)
        ORDER BY g.title
        LIMIT @PageSize OFFSET @Offset;
        """;

    private const string UserLibrarySql = """
        SELECT  li.game_id     AS GameId,
                g.title        AS Title,
                g.genre        AS Genre,
                li.price_paid  AS PricePaid,
                li.acquired_at AS AcquiredAt
        FROM library_items li
        JOIN games g ON g.id = li.game_id
        WHERE li.user_id = @UserId
        ORDER BY li.acquired_at DESC;
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public GameReadQueries(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyCollection<GameCatalogItemResponse>> SearchCatalogAsync(SearchGamesQuery filter, CancellationToken cancellationToken)
    {
        var parameters = new
        {
            filter.Title,
            filter.Genre,
            filter.MinPrice,
            filter.MaxPrice,
            filter.PageSize,
            Offset = (filter.Page - 1) * filter.PageSize
        };

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<GameCatalogItemResponse>(
            new CommandDefinition(SearchCatalogSql, parameters, cancellationToken: cancellationToken));

        return result.ToList();
    }

    public async Task<IReadOnlyCollection<LibraryItemResponse>> GetUserLibraryAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<LibraryItemResponse>(
            new CommandDefinition(UserLibrarySql, new { UserId = userId }, cancellationToken: cancellationToken));

        return result.ToList();
    }
}
