namespace Nexx.Core.ServiceLayer.Setup.Interfaces;

public interface IIntegrationObjectService
{
    Task CreateObjectsAsync(string jsonFilePath);
}