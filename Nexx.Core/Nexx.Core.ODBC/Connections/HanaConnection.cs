// Nexx.Core.ODBC.Hana.ConnectHana.cs
using System.Data;
using System.Data.Odbc;
using Nexx.Core.Logging.Interfaces;
using Nexx.Core.ODBC.Config;
using Nexx.Core.ODBC.Interfaces;

namespace Nexx.Core.ODBC.Connections;

public class HanaConnection : IConnectDb
{
    private readonly ILog<HanaConnection> _log;
    private readonly OdbcConfig _config;

    public HanaConnection(OdbcConfig config, ILog<HanaConnection> log)
    {
        _config = config;
        _log = log;
    }

    public IDbConnection CreateConnection()
    {
        try
        {
            string serverNode = $"{_config.Server}:{_config.Port}";
            string driver = nint.Size == 8 ? "Driver={HDBODBC};" : "Driver={HDBODBC32};";
            string connectionString = $"{driver}ServerNode={serverNode};UID={_config.UserName};PWD={_config.Password};CURRENTSCHEMA={_config.CurrentSchema};databaseName={_config.DataBaseName};";

            var connection = new OdbcConnection(connectionString);
            _log.LogInfo("Conexão SAP HANA ODBC criada.");
            return connection;
        }
        catch (Exception ex)
        {
            _log.LogError("Erro ao criar conexão SAP HANA", ex);
            throw;
        }
    }
}
