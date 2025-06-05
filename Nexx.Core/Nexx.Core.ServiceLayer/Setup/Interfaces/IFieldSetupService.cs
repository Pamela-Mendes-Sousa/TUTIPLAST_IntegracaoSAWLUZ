
namespace Nexx.Core.ServiceLayer.Setup.Interfaces;

public interface IFieldSetupService
{
    Task CreateFieldsAsync(string jsonFilePath);
}
