using System.Net;
using Fcg.Catalog.IntegrationTests._Fixtures;

namespace Fcg.Catalog.IntegrationTests.WebApi._Shared.HealthChecks;

[Collection(CatalogApiCollection.Name)]
public class ReadinessTests
{
    private readonly CatalogApiFactory _factory;

    public ReadinessTests(CatalogApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Ready_DeveRetornarOk_QuandoPostgresEBusDisponiveis()
    {
        var response = await _factory.CreateAnonymousClient().GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
