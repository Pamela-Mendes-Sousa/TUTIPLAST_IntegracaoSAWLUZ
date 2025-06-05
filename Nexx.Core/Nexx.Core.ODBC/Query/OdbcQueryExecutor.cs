using System.Data;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Nexx.Core.Logging.Interfaces;
using Nexx.Core.Mapping.Interfaces;
using Nexx.Core.ODBC.Interfaces;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Nexx.Core.ODBC.Query;

public class OdbcQueryExecutor : IDbQueryExecutor
{
    private readonly IConnectDb _connection;
    private readonly ILog<OdbcQueryExecutor> _log;
    private readonly IServiceProvider _provider;

    public OdbcQueryExecutor(IConnectDb connection, ILog<OdbcQueryExecutor> log, IServiceProvider provider)
    {
        _connection = connection;
        _log = log;
        _provider = provider;
    }

    public Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, params object[] parameters) where T : class, new()
    {
        var mapper = _provider.GetRequiredService<IDataReaderMapper<T>>();
        return ExecuteQueryAsync(sql, mapper, parameters);
    }

    public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, IDataReaderMapper<T> mapper, params object[] parameters) where T : class, new()
    {
        return await ExecuteWithConnection(async conn =>
        {
            var cmd = CreateCommand(conn, sql, parameters);
            using var reader = (DbDataReader)cmd.ExecuteReader();

            var results = new List<T>();
            while (reader.Read())
                results.Add(mapper.Map(reader));

            _log.LogInfo($"Query executada com sucesso: {sql}");
            return results;
        }, sql);
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, IDataReaderMapper<T> mapper, params object[] parameters) where T : class, new()
    {
        return await ExecuteWithConnection(async conn =>
        {
            var cmd = CreateCommand(conn, sql, parameters);
            using var reader = (DbDataReader)cmd.ExecuteReader();

            if (reader.Read())
            {
                var result = mapper.Map(reader);
                _log.LogInfo($"Query (single) executada com sucesso: {sql}");
                return result;
            }

            _log.LogInfo($"Query (single) executada: nenhum resultado encontrado. SQL: {sql}");
            return null;
        }, sql);
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, params object[] parameters) where T : class, new()
    {
        var mapper = _provider.GetRequiredService<IDataReaderMapper<T>>();
        return await QuerySingleAsync(sql, mapper, parameters);
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
    {
        return await ExecuteWithConnection(conn =>
        {
            var cmd = CreateCommand(conn, sql, parameters);
            int affected = cmd.ExecuteNonQuery();
            _log.LogInfo($"ExecuteNonQuery executado com sucesso: {sql}");
            return Task.FromResult(affected);
        }, sql);
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
    {
        return await ExecuteWithConnection(conn =>
        {
            var cmd = CreateCommand(conn, sql, parameters);
            var result = cmd.ExecuteScalar();
            _log.LogInfo($"ExecuteScalar executado com sucesso: {sql}");

            return Task.FromResult((T)Convert.ChangeType(result, typeof(T)));
        }, sql);
    }

    // 🔧 Ajustado para tratar corretamente strings, datas, nulos, números
    private string FormatSqlIfNeeded(string sql, object[] parameters)
    {
        if (parameters?.Length > 0)
        {
            try
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    string placeholder = "{" + i + "}";
                    string value;

                    if (parameters[i] == null)
                    {
                        value = "NULL";
                    }
                    else if (parameters[i] is string str)
                    {
                        value = sql.Contains($"'{placeholder}'")
                            ? str.Replace("'", "''")
                            : $"'{str.Replace("'", "''")}'";
                    }
                    else if (parameters[i] is DateTime dt)
                    {
                        var formatted = dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        value = sql.Contains($"'{placeholder}'")
                            ? formatted
                            : $"'{formatted}'";
                    }
                    else if (parameters[i] is bool b)
                    {
                        value = b ? "1" : "0";
                    }
                    else if (parameters[i] is IFormattable fmt)
                    {
                        value = fmt.ToString(null, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        value = parameters[i].ToString();
                    }

                    sql = sql.Replace(placeholder, value);
                }

                return sql;
            }
            catch (FormatException ex)
            {
                _log.LogError("Erro ao aplicar parâmetros no SQL", ex);
                throw;
            }
        }

        return sql;
    }

    private IDbCommand CreateCommand(IDbConnection connection, string rawSql, object[] parameters)
    {
        var sql = FormatSqlIfNeeded(rawSql, parameters);
        var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return cmd;
    }

    private async Task<T> ExecuteWithConnection<T>(Func<IDbConnection, Task<T>> action, string sql)
    {
        try
        {
            using var conn = _connection.CreateConnection();
            conn.Open();
            return await action(conn);
        }
        catch (Exception ex)
        {
            _log.LogError($"Erro ao executar SQL: {sql}", ex);
            throw;
        }
    }
}
