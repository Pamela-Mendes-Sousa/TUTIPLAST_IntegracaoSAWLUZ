using Nexx.Core.ODBC.Query;
using Nexx.Core.ODBC.Helpers;
using Nexx.Core.ServiceLayer.Client;
using NEXX_SAWLUZIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NEXX_SAWLUZIntegration.Utils;
using Newtonsoft.Json;
using System.Globalization;
using System.Collections.Concurrent;
using System.Drawing;

namespace NEXX_SAWLUZIntegration.Services
{
    public class MarketingDocumentsIntegration
    {
        private readonly ILogger<MarketingDocumentsIntegration> _logger;
        private readonly IDbQueryExecutor _dbQueryExecutor;
        private readonly IServiceLayerClient _serviceLayerClient;

        public MarketingDocumentsIntegration(
            ILogger<MarketingDocumentsIntegration> logger,
            IDbQueryExecutor dbQueryExecutor,
            IServiceLayerClient serviceLayerClient)
        {
            _dbQueryExecutor = dbQueryExecutor;
            _logger = logger;
            _serviceLayerClient = serviceLayerClient;
        }

        public async Task ProcessFileAsync(List<string> arquivosTxt, string pathLanc)
        {
            // Etapa 1: Processamento dos arquivos de forma paralela, mas com escopo isolado por arquivo
            await Task.WhenAll(arquivosTxt.Select(async path =>
            {
                try
                {
                    var linhas = await File.ReadAllLinesAsync(path);

                    var pedidosAgrupados = linhas
                        .Select(linha => AttributeOrders.MapLinhaParaObjeto(linha))
                        .GroupBy(o => o.PedidoNro)
                        .Select(g => new ListAttributeOrders
                        {
                            PedidoNro = g.Key,
                            Orders = g.ToList(),
                            FileName = Path.GetFileName(path)
                        });

                    var pedidosValidos = new List<ListAttributeOrders>();

                    foreach (var pedido in pedidosAgrupados)
                    {
                        if (!await OrderExists(pedido.PedidoNro))
                        {
                            pedidosValidos.Add(pedido);
                        }
                        else
                        {
                            _logger.LogInformation($"Pedido {pedido.PedidoNro} já foi enviado anteriormente. Ignorando.");
                        }
                    }

                    // Etapa 2: Envio em lotes paralelos
                    var loteDoc = Convert.ToInt32(AppConfig.Configuration["LoteDoc"]);
                    var grupos = pedidosValidos.Chunk(loteDoc);

                    var sucessoTotal = true;

                    foreach (var grupo in grupos)
                    {
                        var resultados = await Task.WhenAll(grupo.Select(async pedido =>
                        {
                            var sapObject = new MarketingDocuments();
                            try
                            {
                                sapObject = MapOrdersFields(pedido);
                                var response = await _serviceLayerClient.PostAsync<MarketingDocuments>("Orders", sapObject);
                                _logger.LogInformation($"Pedido {pedido.PedidoNro} enviado com sucesso!");
                                NEXX_LOG log = new NEXX_LOG()
                                {
                                    NEXX_IdDoc = pedido.PedidoNro,
                                    NEXX_TipoDoc = "PedidoVendas",
                                    NEXX_Status = "2",
                                    NEXX_MsgRet = $"Pedido de vendas criado no SAP com sucesso",
                                    NEXX_IdDocLeg = response.DocEntry.ToString(),
                                    NEXX_JsonEnv = sapObject,
                                    NEXX_JsonRet = response,
                                    NEXX_IdRet = path
                                };
                                await log.InsertOrUpdateLog(_dbQueryExecutor);

                                return true;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Erro ao enviar pedido {pedido.PedidoNro}: {ex.Message}");

                                NEXX_LOG log = new NEXX_LOG()
                                {
                                    NEXX_IdDoc = pedido.PedidoNro,
                                    NEXX_TipoDoc = "PedidoVendas",
                                    NEXX_Status = "3",
                                    NEXX_MsgRet = $"Erro ao criar Pedido de vendas : {ex.Message}",
                                    NEXX_JsonEnv = sapObject,
                                    NEXX_JsonRet = ex.ToString(),
                                    NEXX_IdRet = path
                                };
                                await log.InsertOrUpdateLog(_dbQueryExecutor);

                                return false;
                            }
                        }));

                        if (resultados.Any(sucesso => !sucesso))
                        {
                            sucessoTotal = false;
                        }
                    }

                    // Etapa 3: Mover arquivo apenas se todos os pedidos do arquivo foram enviados com sucesso
                    if (sucessoTotal)
                    {
                        var destino = Path.Combine(pathLanc, Path.GetFileName(path));
                        File.Move(path, destino, overwrite: true);
                        _logger.LogInformation($"Arquivo {path} movido para pasta de sucesso.");
                    }
                    else
                    {
                        _logger.LogWarning($"Arquivo {path} NÃO foi movido, pois houve erro em um ou mais pedidos.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro ao processar o arquivo {path}");
                    NEXX_LOG log = new NEXX_LOG()
                    {
                        NEXX_IdDoc = path,
                        NEXX_TipoDoc = "PedidoVendas",
                        NEXX_Status = "3",
                        NEXX_MsgRet = $"Erro ao processar o arquivo {path}",
                        NEXX_JsonRet = ex.ToString(),
                    };
                    await log.InsertOrUpdateLog(_dbQueryExecutor);

                }

            }));
        }

        private MarketingDocuments MapOrdersFields(ListAttributeOrders pedido)
        {
            var marketingDocument = new MarketingDocuments();

            marketingDocument = new MarketingDocuments
            {
                CardCode = pedido.Orders.FirstOrDefault()?.ClienteInterno,
                TaxDate = DateTime.ParseExact(pedido.Orders.FirstOrDefault()?.UE_DtHrEmissao, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                U_IdPrograma = pedido.Orders.FirstOrDefault()?.IdPrograma,
                DocumentLines = pedido.Orders.Select(x => new MarketingDocuments.Documentline
                {
                    ItemCode = x.ProdutoLocal,
                    Quantity = 
                        Convert.ToDouble($"{x.Quantidade.Substring(0, x.Quantidade.Length - 3)}.{x.Quantidade.Substring(x.Quantidade.Length - 3, 3)}", 
                                        CultureInfo.InvariantCulture),
                    VendorNum = x.ProdutoCliente,
                    SupplierCatNum = x.ProdutoCliente,
                    ShipDate = x.DtHrDE != null ? DateTime.ParseExact(x.DtHrDE, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd") : "",
                    U_Fabrica = x.Fabrica,
                    U_PV_PedC = x.PedidoNro,
                    U_Local_PE = x.Local_PE,
                    U_TipoFornec = x.TipoFornecimento,
                    U_CallDelivery = x.CallDelivery,
                    U_UE_Serie = x.UE_Serie
                }).ToList()
            };

            return marketingDocument;
        }

        private async Task<bool> OrderExists(string pedidoNro)
        {
            try
            {
                _logger.LogInformation("Iniciando a busca dos Pedidos no SAP");

                var qr = await SqlQueryLoader.LoadAsync("FindPedidoVenda.sql");
                var pedido = await _dbQueryExecutor.ExecuteQueryAsync<QueryOrderExists>(qr, pedidoNro);

                if (pedido == null || pedido.FirstOrDefault()?.Exist == 0)
                {
                    _logger.LogInformation("Nenhum Pedido de Vendas encontrado.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos no SAP");
                return true; // Por segurança, retorna true para evitar reprocessamento
            }
        }

        public class ListAttributeOrders
        {
            public string PedidoNro { get; set; }
            public List<AttributeOrders> Orders { get; set; }
            public string FileName { get; set; }
        }

        public class QueryOrderExists
        {
            public int Exist { get; set; }
        }
    }
}
