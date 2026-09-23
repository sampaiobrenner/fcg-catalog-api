using System.Text.Json.Serialization;
using Fcg.Catalog.Application._Shared.Security;
using Fcg.Catalog.Domain._Shared.Modules;
using Fcg.Catalog.Infrastructure._Shared.Context;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using Fcg.Catalog.WebApi._Shared.Errors;
using Fcg.Catalog.WebApi._Shared.HealthChecks;
using Fcg.Catalog.WebApi._Shared.Messaging;
using Fcg.Catalog.WebApi._Shared.Security;

namespace Fcg.Catalog.WebApi;

public sealed class FcgCatalogWebApiModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();
        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        services.AddJwtAuthentication(configuration);
        services.AddMessaging(configuration);

        services.AddHealthChecks()
            .AddDbContextCheck<CatalogDbContext>("postgres", tags: [HealthCheckTags.Ready]);

        services.AddEndpoints(typeof(FcgCatalogWebApiModule).Assembly);
    }
}
