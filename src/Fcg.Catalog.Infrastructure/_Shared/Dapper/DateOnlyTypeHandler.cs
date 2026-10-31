using System.Data;
using System.Globalization;
using Dapper;

namespace Fcg.Catalog.Infrastructure._Shared.Dapper;

/// <summary>
/// Converte entre a coluna PostgreSQL <c>date</c> (lida como DateTime pelo Npgsql) e <see cref="DateOnly"/>.
/// Necessario porque o Dapper nao converte DateTime para DateOnly automaticamente.
/// </summary>
internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value) => value switch
    {
        DateOnly dateOnly => dateOnly,
        DateTime dateTime => DateOnly.FromDateTime(dateTime),
        _ => DateOnly.FromDateTime(Convert.ToDateTime(value, CultureInfo.InvariantCulture))
    };
}
