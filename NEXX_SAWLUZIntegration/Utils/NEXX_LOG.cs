using Newtonsoft.Json;
using Nexx.Core.ODBC.Query;
using Nexx.Core.ODBC.Helpers;
using System.Text.Json.Serialization;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
using System.Runtime.CompilerServices;
using Nexx.Core.Logging.Interfaces;

namespace NEXX_SAWLUZIntegration.Utils
{

    public class NEXX_LOG
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        [JsonIgnore]
        public string? ObjectType { get; set; }
        public string? DocEntry { get; set; }
        [JsonProperty("U_NEXX_TipoDoc")]
        public string? NEXX_TipoDoc { get; set; }
        [JsonProperty("U_NEXX_IdDoc")]
        public string? NEXX_IdDoc { get; set; }
        [JsonProperty("U_NEXX_DtInteg")]
        public string? NEXX_DtInteg { get; set; }
        [JsonProperty("U_NEXX_Status")]
        public string? NEXX_Status { get; set; } //1-Pronto 2-Sucesso 3-Erro
        [JsonProperty("U_NEXX_IdRet")]
        public string? NEXX_IdRet { get; set; }
        [JsonProperty("U_NEXX_MsgRet")]
        public string? NEXX_MsgRet { get; set; }
        [JsonProperty("U_NEXX_JsonEnv")]
        public object? NEXX_JsonEnv { get; set; }
        [JsonProperty("U_NEXX_JsonRet")]
        public object? NEXX_JsonRet { get; set; }
        [JsonProperty("U_NEXX_IdDocLeg")]
        public string? NEXX_IdDocLeg { get; set; }
        public int UpdateTime { get; set; }
        public string? UpdateDate { get; set; }


        public async Task  InsertOrUpdateLog(IDbQueryExecutor _dbQueryExecutor)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            var AppName = "NEXX_SAWLUZIntegration";

            if (!string.IsNullOrEmpty(this.ObjectType))
            {

                var sqlObj = await SqlQueryLoader.LoadAsync($@"SelectNEXX_OBJ.sql");
                var nomeObj = await _dbQueryExecutor.QuerySingleAsync<NEXX_OBJ>(sqlObj, this.ObjectType);
                this.NEXX_TipoDoc = nomeObj.U_Nexx_Table_Desc;
            }

            this.NEXX_JsonEnv = this.NEXX_JsonEnv != null ? JsonConvert.SerializeObject(this.NEXX_JsonEnv, settings) : null;
            this.NEXX_JsonRet = this.NEXX_JsonRet != null ? JsonConvert.SerializeObject(this.NEXX_JsonRet, settings) : null;

            try
            {
                //valida se o ID já existe na tabela de log de acordo com o tipo de documento
                var sqlLog = await SqlQueryLoader.LoadAsync($@"SelectNEXX_LOG.sql");
                var exists = await _dbQueryExecutor.QuerySingleAsync<NEXX_LOG>(sqlLog, AppName
                                                                                          , NEXX_TipoDoc
                                                                                          , NEXX_IdDoc);

                if (exists != null)
                {
                    var sqlUP = await SqlQueryLoader.LoadAsync($@"UpdateNEXX_LOG.sql");
                    await _dbQueryExecutor.ExecuteNonQueryAsync(sqlUP, AppName
                                                                      , NEXX_TipoDoc
                                                                      , NEXX_IdDoc
                                                                      , NEXX_DtInteg
                                                                      , NEXX_Status
                                                                      , NEXX_IdRet
                                                                      , NEXX_MsgRet
                                                                      , NEXX_JsonEnv
                                                                      , NEXX_JsonRet
                                                                      , NEXX_IdDocLeg);

                }
                else
                {
                    var sqlIn = await SqlQueryLoader.LoadAsync($@"InsertNEXX_LOG.sql");
                    await _dbQueryExecutor.ExecuteNonQueryAsync(sqlIn, AppName
                                                                      , NEXX_TipoDoc
                                                                      , NEXX_IdDoc
                                                                      , NEXX_DtInteg
                                                                      , NEXX_Status
                                                                      , NEXX_IdRet
                                                                      , NEXX_MsgRet
                                                                      , NEXX_JsonEnv
                                                                      , NEXX_JsonRet
                                                                      , NEXX_IdDocLeg);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    public class NEXX_OBJ
    {
        public string? U_Nexx_Table_Desc { get; set; }
    }
}
