// Nexx.Core.ODBC.Config.OdbcConfig.cs
namespace Nexx.Core.ODBC.Config;

public class OdbcConfig
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string CurrentSchema { get; set; } = string.Empty;
    public string DataBaseName { get; set; } = string.Empty;
}
