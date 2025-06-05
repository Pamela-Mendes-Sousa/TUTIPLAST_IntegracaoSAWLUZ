// Nexx.Core.ODBC.SqlServer.ConnectSqlServer.cs
using System.Data;
using System.Data.Odbc;
using Nexx.Core.Logging;
using Nexx.Core.Logging.Interfaces;
using Nexx.Core.ODBC.Config;
using Nexx.Core.ODBC.Interfaces;

namespace Nexx.Core.ODBC.SqlServer;

public class ConnectSqlServer : IConnectDb
{
    private readonly OdbcConfig _config;
    private readonly ILog<ConnectSqlServer> _log;

    public ConnectSqlServer(OdbcConfig config, ILog<ConnectSqlServer> log)
    {
        _config = config;
        _log = log;
    }

    public IDbConnection CreateConnection()
    {
        try
        {
            string connectionString =
                $"Driver={{ODBC Driver 17 for SQL Server}};" +
                $"Server={_config.Server},{_config.Port};" +
                $"Uid={_config.UserName};Pwd={_config.Password};";

            var connection = new OdbcConnection(connectionString);
            _log.LogInfo("Conexão SQL Server via ODBC criada.");
            return connection;
        }
        catch (Exception ex)
        {
            _log.LogError("Erro ao criar conexão SQL Server", ex);
            throw;
        }
    }
}
