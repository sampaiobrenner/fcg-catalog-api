using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Domain._Shared.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Infrastructure._Shared.Context;

public sealed class CatalogDbContext : DbContext, ICatalogDbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public IQueryable<T> DataSet<T>() where T : PersistenceModelBase => Set<T>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
