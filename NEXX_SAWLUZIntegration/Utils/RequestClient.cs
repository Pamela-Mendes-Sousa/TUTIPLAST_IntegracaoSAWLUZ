using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http;
using NEXX_SAWLUZIntegration.Models;

namespace NEXX_SAWLUZIntegration.Utils
{
    class RequestClient
    {
        public HttpClient client;
        public string Url = AppConfig.Configuration["APIURL"];

        public RequestClient()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(Url);
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<T> Get<T>(string endpoint)
        {


            var response = await this.client.GetAsync(endpoint);
            var json = response.Content.ReadAsStringAsync().Result;


            var classe = JsonConvert.DeserializeObject<T>(json);
            return (T)Convert.ChangeType(classe, typeof(T));


        }

        public async Task<TOuput> Post<TInput, TOuput>(string endpoint, TInput Model)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;

            var JsonInsert = JsonConvert.SerializeObject(Model, settings);

            StringContent json = new StringContent(JsonInsert, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, json);

            var content = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode)
            {
                var newContent = CORPEM_ERP_RESPONSE.CorrectResponse(content);
                var Result = JsonConvert.DeserializeObject<TOuput>(newContent);
                return (TOuput)Convert.ChangeType(Result, typeof(TOuput));
            }
            else
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task Post204<TInput>(string endpoint, TInput Model)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;

            var JsonInsert = JsonConvert.SerializeObject(Model, settings);

            StringContent json = new StringContent(JsonInsert, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, json);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return;
                }
                else
                {
                    throw new Exception($"O servidor retornou um código de status inesperado: {response.StatusCode}");
                }
            }
            else
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<TOuput> Patch<TInput, TOuput>(string endpoint, TInput Model)
        {

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            //settings.DefaultValueHandling = DefaultValueHandling.Ignore;

            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "");
            //client.DefaultRequestHeaders.Add("authClientKey", System.Configuration.ConfigurationManager.AppSettings["authClientKey"]);
            //client.DefaultRequestHeaders.Add("authClientSecret", System.Configuration.ConfigurationManager.AppSettings["authClientSecret"]);

            var JsonInsert = JsonConvert.SerializeObject(Model, settings);
            StringContent json = new StringContent(JsonInsert, Encoding.UTF8, "application/json");

            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, endpoint)
            {
                Content = json
            };

            var response = await client.SendAsync(request);

            var content = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode)
            {
                var Result = JsonConvert.DeserializeObject<TOuput>(content);

                return (TOuput)Convert.ChangeType(Result, typeof(TOuput));
            }
            else
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
