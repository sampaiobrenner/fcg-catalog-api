using Fcg.Catalog.Infrastructure._Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.UnitTests._Fixtures;

/// <summary>Cria um <see cref="CatalogDbContext"/> isolado sobre o provider InMemory para testar projecoes de leitura.</summary>
internal static class InMemoryCatalogDbContext
{
    public static CatalogDbContext Create()
        => new(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"catalog-{Guid.CreateVersion7()}")
            .Options);
}
