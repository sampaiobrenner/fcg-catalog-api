using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fcg.Catalog.Application._Shared.Parsers;

internal static class ParserServiceCollectionExtensions
{
    /// <summary>Registra como singleton cada implementacao concreta de <see cref="IParser{TInput,TOutput}"/> do assembly.</summary>
    public static IServiceCollection AddParsers(this IServiceCollection services, Assembly assembly)
    {
        var registrations = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .SelectMany(type => type.ImplementedInterfaces
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IParser<,>))
                .Select(contract => ServiceDescriptor.Singleton(contract, type)));

        foreach (var registration in registrations)
            services.TryAdd(registration);

        return services;
    }
}
