using System.Net.Http.Headers;
using Fcg.Catalog.Infrastructure._Shared.Context;
using Fcg.Contracts.Security;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Fcg.Catalog.IntegrationTests._Fixtures;

/// <summary>
/// Sobe um PostgreSQL real (Testcontainers), aplica as migrations e o seed no startup da API e troca
/// o RabbitMQ pelo test harness em memoria do MassTransit (outbox/inbox continuam no Postgres).
/// </summary>
public sealed class CatalogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("fcg_catalog")
        .WithUsername("fcg")
        .WithPassword("fcg")
        .Build();

    public ITestHarness Harness => Services.GetTestHarness();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _ = Server; // forca o startup (migrations + seed) antes do primeiro teste
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public HttpClient CreateAnonymousClient() => CreateClient();

    public HttpClient CreateUserClient(Guid userId) => CreateAuthenticatedClient(userId, FcgRoles.User);

    public HttpClient CreateAdminClient() => CreateAuthenticatedClient(Guid.CreateVersion7(), FcgRoles.Administrator);

    public HttpClient CreateAuthenticatedClient(Guid userId, string role)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwtFactory.Create(userId, role));
        return client;
    }

    public async Task ExecuteDbAsync(Func<CatalogDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        await action(scope.ServiceProvider.GetRequiredService<CatalogDbContext>());
    }

    public async Task<T> QueryDbAsync<T>(Func<CatalogDbContext, Task<T>> query)
    {
        await using var scope = Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<CatalogDbContext>());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
        builder.UseSetting("Jwt:Key", TestJwtFactory.SigningKey);
        builder.UseSetting("Jwt:Issuer", TestJwtFactory.Issuer);
        builder.UseSetting("Jwt:Audience", TestJwtFactory.Audience);

        builder.ConfigureTestServices(services => services.AddMassTransitTestHarness(bus =>
            bus.SetTestTimeouts(testTimeout: TimeSpan.FromSeconds(30), testInactivityTimeout: TimeSpan.FromSeconds(5))));
    }
}

[CollectionDefinition(Name)]
public sealed class CatalogApiCollection : ICollectionFixture<CatalogApiFactory>
{
    public const string Name = "catalog-api";
}
