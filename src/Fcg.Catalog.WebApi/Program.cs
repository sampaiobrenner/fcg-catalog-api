using Fcg.Catalog.Application;
using Fcg.Catalog.Domain;
using Fcg.Catalog.Domain._Shared.Modules;
using Fcg.Catalog.Infrastructure;
using Fcg.Catalog.WebApi;
using Fcg.Catalog.WebApi._Shared.Database;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using Fcg.Catalog.WebApi._Shared.HealthChecks;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logger) => logger.ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddModule<FcgCatalogDomainModule>(builder.Configuration)
    .AddModule<FcgCatalogApplicationModule>(builder.Configuration)
    .AddModule<FcgCatalogInfrastructureModule>(builder.Configuration)
    .AddModule<FcgCatalogWebApiModule>(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsAsync(app.Lifetime.ApplicationStopping);

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapHealthEndpoints();
app.MapEndpoints();

await app.RunAsync();

public partial class Program;
