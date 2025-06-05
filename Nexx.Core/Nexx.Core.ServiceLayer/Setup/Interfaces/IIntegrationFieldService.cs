namespace Nexx.Core.ServiceLayer.Setup.Interfaces;

public interface IIntegrationFieldService
{
    Task CreateFieldsAsync(string jsonFilePath);
}