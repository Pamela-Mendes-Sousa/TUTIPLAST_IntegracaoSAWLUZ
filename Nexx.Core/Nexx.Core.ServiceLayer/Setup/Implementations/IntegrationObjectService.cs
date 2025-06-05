using Nexx.Core.Logging.Interfaces;
using Nexx.Core.ServiceLayer.Client;
using Nexx.Core.ServiceLayer.Setup.Helpers;
using Nexx.Core.ServiceLayer.Setup.Interfaces;
using Nexx.Core.ServiceLayer.Setup.Models;
using System.Text.Json;

public class IntegrationUserObjectService : IIntegrationObjectService
{
    private readonly IServiceLayerClient _client;
    private readonly ILog<IntegrationUserObjectService> _log;

    public IntegrationUserObjectService(IServiceLayerClient client, ILog<IntegrationUserObjectService> log)
    {
        _client = client;
        _log = log;
    }

    public async Task CreateObjectsAsync(string jsonFilePath)
    {
        var (_,_,userObjects) = IntegrationSchemaLoader.LoadFromJson(jsonFilePath);

        if (userObjects == null || !userObjects.Any())
        {
            _log.LogInfo("Nenhum objeto de usuário definido para criação.");
            return;
        }

        foreach (var udo in userObjects)
        {
            try
            {
                var response = await _client.GetRawAsync($"UserObjectsMD?$filter=TableName eq '{udo.TableName}'");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);
                var exists = jsonDoc.RootElement
                                    .GetProperty("value")
                                    .EnumerateArray()
                                    .Any();

                if (exists)
                {
                    _log.LogInfo($"UDO {udo.TableName} já existe.");
                    continue;
                }

                var result = await _client.PostRawAsync("UserObjectsMD", udo);
                var responseBody = await result.Content.ReadAsStringAsync();

                if (!result.IsSuccessStatusCode)
                {
                    _log.LogError($"Erro ao criar UDO {udo.TableName}: {(int)result.StatusCode} - {result.ReasonPhrase}");
                    _log.LogError($"Detalhes do erro:\n{responseBody}");
                    continue;
                }

                _log.LogInfo($"UDO {udo.TableName} criado com sucesso.");
            }
            catch (Exception ex)
            {
                _log.LogError($"Erro inesperado ao criar o UDO {udo.TableName}", ex);
            }
        }
    }
}
