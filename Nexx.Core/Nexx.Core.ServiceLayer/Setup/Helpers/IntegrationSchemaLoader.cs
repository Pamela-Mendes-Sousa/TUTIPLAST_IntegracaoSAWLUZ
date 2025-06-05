using System.Text.Json;
using Nexx.Core.ServiceLayer.Setup.Models;

namespace Nexx.Core.ServiceLayer.Setup.Helpers;

public static class IntegrationSchemaLoader
{
    public static (List<IntegrationTableModel> Tables, List<IntegrationFieldModel> Fields, List<IntegrationObjectModel> Objects) LoadFromJson(string path)
    {
        var json = File.ReadAllText(path);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null 
        };

        var tables = JsonSerializer.Deserialize<List<IntegrationTableModel>>(root.GetProperty("UserTables"), options) ?? [];
        var fields = JsonSerializer.Deserialize<List<IntegrationFieldModel>>(root.GetProperty("UserFields"), options) ?? [];
        var userObject = JsonSerializer.Deserialize<List<IntegrationObjectModel>>(root.GetProperty("UserObjects"), options) ?? [];

        return (tables, fields, userObject);
    }
}
