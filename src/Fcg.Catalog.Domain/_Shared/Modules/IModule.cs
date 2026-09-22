using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Catalog.Domain._Shared.Modules;

public interface IModule
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
