using Nexx.Core.Logging.Interfaces;
using Nexx.Core.ServiceLayer.Config;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Nexx.Core.ServiceLayer.Auth;

public class ServiceLayerAuth : IServiceLayerAuth
{
    private readonly HttpClient _httpClient;
    private readonly ServiceLayerConfig _config;
    private readonly ILog<ServiceLayerAuth> _log;

    private string? _sessionId;
    private string? _routeId;
    private DateTime _expiresAt;

    private readonly SemaphoreSlim _lock = new(1, 1);

    public ServiceLayerAuth(HttpClient httpClient, ServiceLayerConfig config, ILog<ServiceLayerAuth> log)
    {
        _httpClient = httpClient;
        _config = config;
        _log = log;
    }

    public async Task<(string SessionId, string RouteId)> GetSessionAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_sessionId) && _expiresAt > DateTime.UtcNow)
            {
                return (_sessionId, _routeId!);
            }

            _log.LogInfo("Realizando autenticação no Service Layer...");

            var loginBody = new
            {
                CompanyDB = _config.CompanyDB,
                UserName = _config.UserName,
                Password = _config.Password,
                Language = 29 // Português do Brasil
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(loginBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/b1s/v2/Login", requestContent);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError($"Erro ao autenticar: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                throw new Exception("Falha ao autenticar no Service Layer");
            }

            var cookies = response.Headers.TryGetValues("Set-Cookie", out var cookieValues)
                ? cookieValues.ToList()
                : throw new Exception("Cookies de sessão não retornados.");

            _sessionId = ExtractCookieValue(cookies, "B1SESSION");
            try
            {
                _routeId = ExtractCookieValue(cookies, "ROUTEID");
            }
            catch
            {
                _routeId = null;
                _log.LogWarning("Cookie ROUTEID não retornado. Ambiente pode estar sem balanceador.");
            }

            _expiresAt = DateTime.UtcNow.AddMinutes(25); // Sessão padrão ~30 min, margem de segurança

            _log.LogInfo("Sessão autenticada com sucesso no Service Layer.");

            return (_sessionId, _routeId!);

        }
        finally
        {
            _lock.Release();
        }
    }

    public HttpClient GetAuthenticatedClient()
    {
        // Sincroniza o acesso à sessão e força renovação se necessário
        var task = Task.Run(async () => await GetAuthenticatedClientAsync());
        return task.GetAwaiter().GetResult();
    }

    public async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var (sessionId, routeId) = await GetSessionAsync();

        await _lock.WaitAsync();
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("Cookie");
            var cookieHeader = $"B1SESSION={sessionId}";
            if (!string.IsNullOrEmpty(routeId))
                cookieHeader += $"; ROUTEID={routeId}";
            _httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            return _httpClient;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static string ExtractCookieValue(List<string> cookies, string name)
    {
        var cookie = cookies.FirstOrDefault(c => c.Contains(name));
        if (cookie == null) throw new Exception($"Cookie {name} não encontrado.");
        return cookie.Split(';').FirstOrDefault(c => c.Contains(name))?.Split('=').Last()?.Trim() ?? string.Empty;
    }
}