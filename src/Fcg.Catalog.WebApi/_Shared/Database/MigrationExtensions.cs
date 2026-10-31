using Fcg.Catalog.Infrastructure._Shared.Context;
using Fcg.Catalog.Infrastructure._Shared.Context.Seed;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.WebApi._Shared.Database;

public static class MigrationExtensions
{
    private const string ApplyMigrationsOnStartupKey = "Database:ApplyMigrationsOnStartup";

    /// <summary>Aplica as migrations pendentes e, em seguida, o seed idempotente do catalogo.</summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app, CancellationToken cancellationToken)
    {
        if (!app.Configuration.GetValue<bool>(ApplyMigrationsOnStartupKey))
            return;

        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await context.Database.MigrateAsync(cancellationToken);

        await scope.ServiceProvider.GetRequiredService<CatalogDatabaseSeeder>().SeedAsync(cancellationToken);
    }
}
