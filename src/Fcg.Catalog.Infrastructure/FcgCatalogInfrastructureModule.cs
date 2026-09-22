using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Messaging;
using Fcg.Catalog.Domain._Shared.Modules;
using Fcg.Catalog.Infrastructure._Shared.Context;
using Fcg.Catalog.Infrastructure._Shared.Messaging;
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
    }
}
