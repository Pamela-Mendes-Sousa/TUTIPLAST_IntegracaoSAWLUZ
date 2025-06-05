namespace Nexx.Core.ServiceLayer.Setup.Interfaces;

public interface IIntegrationTableService
{
    Task CreateTablesAsync(string jsonFilePath);
}