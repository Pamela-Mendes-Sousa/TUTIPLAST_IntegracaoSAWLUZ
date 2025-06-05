
namespace Nexx.Core.ServiceLayer.Setup.Interfaces;

public interface ITableSetupService
{
    Task CreateTablesAsync(string jsonFilePath);
}
