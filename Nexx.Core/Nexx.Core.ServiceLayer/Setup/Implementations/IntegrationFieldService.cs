using System.Text.Json;
using Nexx.Core.Logging.Interfaces;
using Nexx.Core.ServiceLayer.Setup.Interfaces;
using Nexx.Core.ServiceLayer.Setup.Helpers;
using Nexx.Core.ServiceLayer.Setup.Models;
using Nexx.Core.ServiceLayer.Client;

namespace Nexx.Core.ServiceLayer.Setup.Implementations;

public class IntegrationFieldService : IIntegrationFieldService
{
    private readonly IServiceLayerClient _client;
    private readonly ILog<IntegrationFieldService> _log;

    public IntegrationFieldService(IServiceLayerClient client, ILog<IntegrationFieldService> log)
    {
        _client = client;
        _log = log;
    }

    public async Task CreateFieldsAsync(string jsonFilePath)
    {
        (_, List<IntegrationFieldModel> fields,_) = IntegrationSchemaLoader.LoadFromJson(jsonFilePath);

        if (!fields.Any())
        {
            _log.LogInfo("Nenhum campo definido para criação.");
            return;
        }

        foreach (var field in fields)
        {
            try
            {
                if (await FieldExistsAsync(field.TableName, field.Name))
                {
                    _log.LogInfo($"Campo {field.Name} já existe na tabela {field.TableName}.");
                    continue;
                }

                //var payload = new Dictionary<string, object>
                //{
                //    { "TableName", field.TableName },
                //    { "Name", field.Name },
                //    { "Description", field.Description },
                //    { "Type", field.Type }
                //};

                //// Regra por tipo de campo
                //switch (field.Type)
                //{
                //    case "db_Alpha":
                //        payload["Size"] = field.Size > 0 ? field.Size : 50;
                //        payload["EditSize"] = field.EditSize > 0 ? field.EditSize : 50;
                //        break;

                //    case "db_Numeric":
                //        payload["EditSize"] = field.EditSize > 0 ? field.EditSize : 11;
                //        break;

                //        // db_Memo e db_Date não exigem tamanhos
                //}

                _log.LogInfo("Enviando field para criação do campo:");
                _log.LogInfo(JsonSerializer.Serialize(field, new JsonSerializerOptions { WriteIndented = true }));

                var response = await _client.PostRawAsync("UserFieldsMD", field);
                response.EnsureSuccessStatusCode();

                _log.LogInfo($"Campo {field.Name} criado com sucesso na tabela {field.TableName}.");
            }
            catch (Exception ex)
            {
                _log.LogError($"Erro ao criar campo {field.Name} na tabela {field.TableName}", ex);
            }
        }
    }

    private async Task<bool> FieldExistsAsync(string tableName, string fieldName)
    {
        var url = $"UserFieldsMD?$filter=TableName eq '{tableName}' and Name eq '{fieldName}'";
        var response = await _client.GetRawAsync(url);
        if (!response.IsSuccessStatusCode) return false;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("value").EnumerateArray().Any();
    }
}
