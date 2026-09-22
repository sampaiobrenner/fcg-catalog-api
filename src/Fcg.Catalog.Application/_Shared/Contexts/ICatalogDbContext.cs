using Fcg.Catalog.Domain._Shared.Models;

namespace Fcg.Catalog.Application._Shared.Contexts;

public interface ICatalogDbContext
{
    IQueryable<T> DataSet<T>() where T : PersistenceModelBase;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
