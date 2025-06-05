using System.Text.Json;
using Nexx.Core.Mapping.Interfaces;

namespace Nexx.Core.Mapping.Json;

public class JsonMapper<T> : IJsonMapper<T> where T : class
{
    public T Map(string json)
    {
        var result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result is null)
            throw new InvalidOperationException($"Falha ao deserializar JSON para {typeof(T).Name}");

        return result;
    }
}
