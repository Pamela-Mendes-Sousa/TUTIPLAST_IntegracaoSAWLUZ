using System.Net.Http;
using System.Threading.Tasks;

namespace Nexx.Core.ServiceLayer.Client
{
    /// <summary>
    /// Cliente de acesso ao SAP Business One Service Layer.
    /// Gerencia operações HTTP com suporte para retorno tipado e bruto.
    /// </summary>
    public interface IServiceLayerClient
    {
        /// <summary>
        /// Executa uma requisição GET e desserializa o conteúdo para o tipo informado.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto esperado na resposta.</typeparam>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <returns>Objeto desserializado da resposta.</returns>
        Task<T> GetAsync<T>(string endpoint);

        /// <summary>
        /// Executa uma requisição POST enviando um payload e desserializa o conteúdo da resposta.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto esperado na resposta.</typeparam>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <param name="payload">Objeto a ser enviado como corpo da requisição.</param>
        /// <returns>Objeto desserializado da resposta.</returns>
        Task<T> PostAsync<T>(string endpoint, object payload);

        /// <summary>
        /// Executa uma requisição PATCH enviando um payload e desserializa o conteúdo da resposta.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto esperado na resposta.</typeparam>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <param name="payload">Objeto a ser enviado como corpo da requisição.</param>
        /// <returns>Objeto desserializado da resposta.</returns>
        Task<T> PatchAsync<T>(string endpoint, object payload);

        /// <summary>
        /// Executa uma requisição DELETE no endpoint informado.
        /// </summary>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        Task DeleteAsync(string endpoint);

        /// <summary>
        /// Executa uma requisição GET retornando a resposta bruta (HttpResponseMessage).
        /// </summary>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <returns>Resposta HTTP bruta.</returns>
        Task<HttpResponseMessage> GetRawAsync(string endpoint);

        /// <summary>
        /// Executa uma requisição POST retornando a resposta bruta (HttpResponseMessage).
        /// </summary>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <param name="payload">Objeto a ser enviado como corpo da requisição.</param>
        /// <returns>Resposta HTTP bruta.</returns>
        Task<HttpResponseMessage> PostRawAsync(string endpoint, object payload);

        /// <summary>
        /// Executa uma requisição PATCH retornando a resposta bruta (HttpResponseMessage).
        /// </summary>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <param name="payload">Objeto a ser enviado como corpo da requisição.</param>
        /// <returns>Resposta HTTP bruta.</returns>
        Task<HttpResponseMessage> PatchRawAsync(string endpoint, object payload);

        /// <summary>
        /// Executa uma requisição DELETE retornando a resposta bruta (HttpResponseMessage).
        /// </summary>
        /// <param name="endpoint">Endpoint relativo no Service Layer.</param>
        /// <returns>Resposta HTTP bruta.</returns>
        Task<HttpResponseMessage> DeleteRawAsync(string endpoint);
    }
}