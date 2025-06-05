public interface IServiceLayerAuth
{
    Task<(string SessionId, string RouteId)> GetSessionAsync();
    HttpClient GetAuthenticatedClient();
    Task<HttpClient> GetAuthenticatedClientAsync();

}