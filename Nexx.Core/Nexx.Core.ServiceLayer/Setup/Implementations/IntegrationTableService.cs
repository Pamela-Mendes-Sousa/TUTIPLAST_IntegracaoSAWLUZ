using Nexx.Core.Logging.Interfaces;
using Nexx.Core.ServiceLayer.Setup.Interfaces;
using Nexx.Core.ServiceLayer.Setup.Helpers;
using Nexx.Core.ServiceLayer.Client;
using Nexx.Core.ServiceLayer.Setup.Models;
using System.Text.Json;

namespace Nexx.Core.ServiceLayer.Setup.Implementations;

/// <summary>
/// Cria tabelas de usuário via Service Layer.
/// </summary>
public class IntegrationTableService : IIntegrationTableService
{
    private readonly IServiceLayerClient _client;
    private readonly ILog<IntegrationTableService> _log;

    public IntegrationTableService(IServiceLayerClient client, ILog<IntegrationTableService> log)
    {
        _client = client;
        _log = log;
    }

    public async Task CreateTablesAsync(string jsonFilePath)
    {
        var (tables, _,_) = IntegrationSchemaLoader.LoadFromJson(jsonFilePath);

        if (!tables.Any())
        {
            _log.LogInfo("Nenhuma tabela definida para criação.");
            return;
        }

        foreach (var table in tables)
        {
            try
            {
                var response = await _client.GetRawAsync($"UserTablesMD?$filter=TableName eq '{table.TableName}'");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);
                var exists = jsonDoc.RootElement
                                    .GetProperty("value")
                                    .EnumerateArray()
                                    .Any();

                if (exists)
                {
                    _log.LogInfo($"Tabela {table.TableName} já existe.");
                    continue;
                }

                // 🔥 IMPORTANTE: Aqui usa diretamente PostAsync<T> original (como era antes)
                var result = await _client.PostRawAsync("UserTablesMD", table);
                var responseBody = await result.Content.ReadAsStringAsync();

                if (!result.IsSuccessStatusCode)
                {
                    _log.LogError($"Erro ao criar tabela {table.TableName}: {(int)result.StatusCode} - {result.ReasonPhrase}");
                    _log.LogError($"Detalhes do erro:\n{responseBody}");
                    continue;
                }

                _log.LogInfo($"Tabela {table.TableName} criada com sucesso.");
            }
            catch (Exception ex)
            {
                _log.LogError($"Erro inesperado ao criar a tabela {table.TableName}", ex);
            }
        }
    }
}
