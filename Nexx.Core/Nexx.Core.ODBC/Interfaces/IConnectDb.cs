using System.Data;

namespace Nexx.Core.ODBC.Interfaces;

public interface IConnectDb
{
    IDbConnection CreateConnection();
}
