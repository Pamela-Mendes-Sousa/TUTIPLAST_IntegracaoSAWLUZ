using System.Collections.Generic;
using System.Threading.Tasks;
using Nexx.Core.ODBC.Interfaces;
using System.Data.Common;
using Nexx.Core.Mapping.Interfaces;
using System.Data;

namespace Nexx.Core.ODBC.Query;

public interface IDbQueryExecutor
{
    Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, params object[] parameters)
        where T : class, new();

    Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, IDataReaderMapper<T> mapper, params object[] parameters)
        where T : class, new();

    Task<T?> QuerySingleAsync<T>(string sql, IDataReaderMapper<T> mapper, params object[] parameters) where T : class, new();

    Task<T?> QuerySingleAsync<T>(string sql, params object[] parameters) where T : class, new();

    Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters);

    Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters);
}
