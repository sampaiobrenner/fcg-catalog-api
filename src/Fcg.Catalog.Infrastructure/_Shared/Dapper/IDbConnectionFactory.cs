using System.Data.Common;

namespace Fcg.Catalog.Infrastructure._Shared.Dapper;

/// <summary>Cria conexoes abertas para as consultas Dapper.</summary>
internal interface IDbConnectionFactory
{
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}
