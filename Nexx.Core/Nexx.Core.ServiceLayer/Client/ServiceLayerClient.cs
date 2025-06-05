using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nexx.Core.Logging.Interfaces;
using Nexx.Core.ServiceLayer.Auth;
using Nexx.Core.ServiceLayer.Setup.Helpers;

namespace Nexx.Core.ServiceLayer.Client
{
    public class ServiceLayerClient : IServiceLayerClient
    {
        private readonly IServiceLayerAuth _auth;
        private readonly ILog<ServiceLayerClient> _log;

        private static readonly JsonSerializerOptions LogJsonOptions = new()
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull
        };

        private const int MaxLogContentLength = 5000;

        public ServiceLayerClient(IServiceLayerAuth auth, ILog<ServiceLayerClient> log)
        {
            _auth = auth;
            _log = log;
        }

        // ==================== Métodos Tipados ====================

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await SendRequestAsync(HttpMethod.Get, endpoint);
            return await DeserializeResponseAsync<T>(response, endpoint);
        }

        public async Task<T> PostAsync<T>(string endpoint, object payload)
        {
            var response = await SendRequestAsync(HttpMethod.Post, endpoint, payload);
            return await DeserializeResponseAsync<T>(response, endpoint);
        }

        public async Task<T> PatchAsync<T>(string endpoint, object payload)
        {
            var response = await SendRequestAsync(HttpMethod.Patch, endpoint, payload);

            return await DeserializeResponseAsync<T>(response, endpoint);
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await SendRequestAsync(HttpMethod.Delete, endpoint);
            response.EnsureSuccessStatusCode();
            await LogResponseContentAsync(response, endpoint);
        }

        // ==================== Métodos Crus ====================

        public async Task<HttpResponseMessage> GetRawAsync(string endpoint)
            => await SendRequestAsync(HttpMethod.Get, endpoint);

        public async Task<HttpResponseMessage> PostRawAsync(string endpoint, object payload)
            => await SendRequestAsync(HttpMethod.Post, endpoint, payload);

        public async Task<HttpResponseMessage> PatchRawAsync(string endpoint, object payload)
            => await SendRequestAsync(HttpMethod.Patch, endpoint, payload);

        public async Task<HttpResponseMessage> DeleteRawAsync(string endpoint)
            => await SendRequestAsync(HttpMethod.Delete, endpoint);

        // ==================== Internos Auxiliares ====================

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string endpoint, object? payload = null)
        {
            var client = await _auth.GetAuthenticatedClientAsync();
            _log.LogInfo($"{method.Method} -> {endpoint}");

            var request = new HttpRequestMessage(method, endpoint);

            if (payload != null)
            {
                string payloadJson = JsonSerializer.Serialize(payload, LogJsonOptions);
                _log.LogInfo($"Payload Enviado [{method.Method} {endpoint}]:\n{payloadJson}");

                request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            }
            var response = await client.SendAsync(request);
            return response;
        }

        private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response, string endpoint)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await LogResponseContentAsync(response, endpoint, content);
                // Captura detalhes do erro do SAP
                var sapError = TryParseSapError(content);
                throw new Exception($"Erro do SAP Service Layer em {endpoint}: {sapError}");
            }

            if (response.StatusCode == HttpStatusCode.NoContent || string.IsNullOrEmpty(content))
            {
                // Se esperava uma lista, retorna uma instância vazia de List<...>
                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                {
                    return (T)Activator.CreateInstance(typeof(T))!;
                }

                // Para tipos escalares ou objetos, default(T) (null para classes, 0 para int, etc.)
                return default!;
            }

            await LogResponseContentAsync(response, endpoint, content);

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            bool isListExpected = typeof(T).IsGenericType &&
                                  typeof(T).GetGenericTypeDefinition() == typeof(List<>);

            if (isListExpected)
            {
                if (!root.TryGetProperty("value", out var valueElement))
                    throw new InvalidOperationException($"Esperado campo 'value' no retorno da lista: {endpoint}");

                return JsonSerializer.Deserialize<T>(valueElement.GetRawText(), JsonOptionsProvider.PascalCaseOptions)
                       ?? throw new InvalidOperationException($"Falha ao desserializar a lista em: {endpoint}");
            }

            return JsonSerializer.Deserialize<T>(content, JsonOptionsProvider.PascalCaseOptions)
                   ?? throw new InvalidOperationException($"Falha ao desserializar o objeto em: {endpoint}");
        }

        private async Task LogResponseContentAsync(HttpResponseMessage response, string endpoint, string? content = null)
        {
            if (content == null)
            {
                content = await response.Content.ReadAsStringAsync();
            }

            var prettyContent = TryFormatJson(content);

            if (prettyContent.Length > MaxLogContentLength)
            {
                prettyContent = prettyContent.Substring(0, MaxLogContentLength) + "... [truncated]";
            }

            _log.LogInfo($"Resposta [{(int)response.StatusCode} {response.ReasonPhrase}] de {endpoint}:\n{prettyContent}");
        }

        private string TryFormatJson(string content)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(content);
                return JsonSerializer.Serialize(jsonDoc, LogJsonOptions);
            }
            catch
            {
                // Se não for um JSON válido, retorna o conteúdo cru
                return content;
            }
        }

        private string TryParseSapError(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "Resposta vazia do Service Layer";

            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.TryGetProperty("error", out var error))
                {
                    // 1) Código numérico de erro (quando houver)
                    int? code = null;
                    if (error.TryGetProperty("code", out var codeElem) && codeElem.ValueKind == JsonValueKind.Number)
                        code = codeElem.GetInt32();

                    // 2) Mensagem principal: pode ser string direta ou objeto com .value
                    string messageText = null;
                    if (error.TryGetProperty("message", out var message))
                    {
                        switch (message.ValueKind)
                        {
                            case JsonValueKind.String:
                                messageText = message.GetString();
                                break;

                            case JsonValueKind.Object:
                                if (message.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.String)
                                    messageText = val.GetString();
                                break;
                        }
                    }

                    // 3) Se não achou mensagem, tenta innererror.application.internalerror.message
                    if (string.IsNullOrEmpty(messageText)
        && error.TryGetProperty("innererror", out var inner)
        && inner.TryGetProperty("application", out var app)
        && app.TryGetProperty("internalerror", out var internalErr)
        && internalErr.TryGetProperty("message", out var innerMsg)
        && innerMsg.ValueKind == JsonValueKind.String)
                    {
                        messageText = innerMsg.GetString();
                    }

                    // 4) Fallback final
                    if (string.IsNullOrEmpty(messageText))
                        messageText = "Erro desconhecido";

                    // 5) Monta resultado com código, se houver
                    return code.HasValue
                        ? $"{code.Value}: {messageText}"
                        : messageText;
                }
            }
            catch (JsonException)
            {
                // conteúdo não era JSON válido
            }

            return "Erro desconhecido ou resposta inválida do Service Layer";
        }
    }
}