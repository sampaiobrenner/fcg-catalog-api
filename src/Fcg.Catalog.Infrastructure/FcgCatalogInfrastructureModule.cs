using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Messaging;
using Fcg.Catalog.Application._Shared.ReadQueries;
using Fcg.Catalog.Domain._Shared.Modules;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Library;
using Fcg.Catalog.Domain.Orders;
using Fcg.Catalog.Domain.Promotions;
using Fcg.Catalog.Infrastructure._Shared.Context;
using Fcg.Catalog.Infrastructure._Shared.Context.Seed;
using Fcg.Catalog.Infrastructure._Shared.Dapper;
using Fcg.Catalog.Infrastructure._Shared.Messaging;
using Fcg.Catalog.Infrastructure.Games;
using Fcg.Catalog.Infrastructure.Library;
using Fcg.Catalog.Infrastructure.Orders;
using Fcg.Catalog.Infrastructure.Promotions;
using Fcg.Catalog.Infrastructure.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Fcg.Catalog.Infrastructure;

public sealed class FcgCatalogInfrastructureModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(InfrastructureResources.ConnectionStringAusente);

        var npgsqlConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            GssEncryptionMode = GssEncryptionMode.Disable
        }.ConnectionString;

        services.AddDbContext<CatalogDbContext>(options => options
            .UseNpgsql(npgsqlConnectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<ICatalogDbContext>(provider => provider.GetRequiredService<CatalogDbContext>());
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
        services.AddScoped<CatalogDatabaseSeeder>();

        services.AddSingleton<IDbConnectionFactory>(new NpgsqlConnectionFactory(npgsqlConnectionString));
        services.AddScoped<IGameReadQueries, GameReadQueries>();

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<ILibraryRepository, LibraryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
    }
}
