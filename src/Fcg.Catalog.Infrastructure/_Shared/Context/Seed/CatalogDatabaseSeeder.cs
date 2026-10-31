using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Games;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Infrastructure._Shared.Context.Seed;

/// <summary>
/// Popula o catalogo com os jogos de exemplo do monolito. Idempotente: so insere quando a tabela
/// <c>games</c> esta vazia. Um advisory lock transacional evita que replicas subindo juntas
/// insiram os jogos em duplicidade.
/// </summary>
public sealed class CatalogDatabaseSeeder
{
    private const long SeedLockKey = 7_020_250_922;

    private readonly CatalogDbContext _context;

    public CatalogDatabaseSeeder(CatalogDbContext context) => _context = context;

    internal static IReadOnlyCollection<Game> DefaultGames() =>
    [
        Game.Create("The Witcher 3: Wild Hunt", "RPG de mundo aberto", "RPG", Money.Create(79.90m), new DateOnly(2015, 5, 19)),
        Game.Create("Hades", "Roguelike de ação", "Ação", Money.Create(49.90m), new DateOnly(2020, 9, 17)),
        Game.Create("Stardew Valley", "Simulador de fazenda", "Simulação", Money.Create(24.90m), new DateOnly(2016, 2, 26)),
        Game.Create("Celeste", "Plataforma desafiador", "Plataforma", Money.Create(37.90m), new DateOnly(2018, 1, 25))
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({SeedLockKey})", cancellationToken);

        if (!await _context.Set<GamePersistenceModel>().AnyAsync(cancellationToken))
        {
            await _context.Set<GamePersistenceModel>().AddRangeAsync(DefaultGames().Select(game => game.PersistenceModel), cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
