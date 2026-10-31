using System.Data.Common;
using Dapper;
using Npgsql;

namespace Fcg.Catalog.Infrastructure._Shared.Dapper;

internal sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    static NpgsqlConnectionFactory()
    {
        // Mapeia colunas snake_case (release_date) para propriedades PascalCase (ReleaseDate).
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        // Permite materializar colunas 'date' em propriedades DateOnly.
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    public NpgsqlConnectionFactory(string connectionString) => _connectionString = connectionString;

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
