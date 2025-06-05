using System.Net.Http;
using System.Threading.Tasks;

namespace Nexx.Core.ServiceLayer.Client
{
    /// <summary>
    /// Cliente de acesso ao SAP Business One Service Layer.
    /// Gerencia opera��es HTTP com suporte para retorno tipado e bruto.
    /// </summary>
    public interface IServiceLayerClient
    {
        /// <summary>
        /// Executa uma requisi��o GET e desserializa o conte�do para o tipo informado.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto esperado na resposta.</typeparam>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <returns>Objeto desserializado da resposta.</returns>
        Task<T> GetAsync<T>(string endpoint);

        /// <summary>
        /// Executa uma requisi��o POST enviando um payload e desserializa o conte�do da resposta.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto esperado na resposta.</typeparam>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <param name="payload">Objeto a ser enviado como corpo da requisi��o.</param>
        /// <returns>Objeto desserializado da resposta.</returns>
        Task<T> PostAsync<T>(string endpoint, object payload);

        /// <summary>
        /// Executa uma requisi��o PATCH enviando um payload e desserializa o conte�do da resposta.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto esperado na resposta.</typeparam>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <param name="payload">Objeto a ser enviado como corpo da requisi��o.</param>
        /// <returns>Objeto desserializado da resposta.</returns>
        Task<T> PatchAsync<T>(string endpoint, object payload);

        /// <summary>
        /// Executa uma requisi��o DELETE no endpoint informado.
        /// </summary>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        Task DeleteAsync(string endpoint);

        /// <summary>
        /// Executa uma requisi��o GET retornando a resposta bruta (HttpResponseMessage).
        /// </summary>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <returns>Resposta HTTP bruta.</returns>
        Task<HttpResponseMessage> GetRawAsync(string endpoint);

        /// <summary>
        /// Executa uma requisi��o POST retornando a resposta bruta (HttpResponseMessage).
        /// </summary>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <param name="payload">Objeto a ser enviado como corpo da requisi��o.</param>
        /// <returns>Resposta HTTP bruta.</returns>
        Task<HttpResponseMessage> PostRawAsync(string endpoint, object payload);

        /// <summary>
        /// Executa uma requisi��o PATCH retornando a resposta bruta (HttpResponseMessage).
        /// </summary>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <param name="payload">Objeto a ser enviado como corpo da requisi��o.</param>
        /// <returns>Resposta HTTP bruta.</returns>
        Task<HttpResponseMessage> PatchRawAsync(string endpoint, object payload);

        /// <summary>
        /// Executa uma requisi��o DELETE retornando a resposta bruta (HttpResponseMessage).
        /// </summary>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <returns>Resposta HTTP bruta.</returns>
        Task<HttpResponseMessage> DeleteRawAsync(string endpoint);
    }
}