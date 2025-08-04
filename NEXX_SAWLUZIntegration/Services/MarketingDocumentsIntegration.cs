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
using System.IO;

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

                    var tipoDocAgrupados = linhas
                        .Select(linha => AttributeOrders.MapLinhaParaObjeto(linha))
                        .GroupBy(o => new { o.Orig_PE_PD })
                        .Select(g => new ListAttributeOrders
                        {
                            TipoDoc = g.Key.Orig_PE_PD,
                            Orders = g.ToList(),
                            FileName = Path.GetFileName(path)
                        });

                    int erro = 0;
                    foreach (var pedido in tipoDocAgrupados.Where(x => !string.IsNullOrEmpty(x.TipoDoc)))
                    {
                        _logger.LogInformation($"Validando. Pedido {pedido.Orders.FirstOrDefault().PedidoNro}");
                        var ret = await OrderExists(pedido.Orders.FirstOrDefault().PedidoNro, pedido.TipoDoc);
                        if (ret.DocEntry == 0)
                        {
                            _logger.LogInformation($"Pedido {pedido.Orders.FirstOrDefault().PedidoNro} nao existe, realizando inserção");
                            if (!await CreateDocument(pedido, path))
                                erro++;
                        }
                        else
                        {
                            //.ACHEI UM DOC QUE POSSUI AQUELE NUMERO DE PEDIDO E SUBSTITUI TODAS AS LINHAS
                            _logger.LogInformation($"Pedido {pedido.Orders.FirstOrDefault().PedidoNro} DocEntry {ret.DocEntry} e DocNum {ret.DocNum}já foi enviado anteriormente. Atualizando.");
                            if (!await UpdateDocument(pedido, path, ret.DocEntry.ToString(), ret.DocNum.ToString()))
                                erro++;
                        }

                    }

                    // Etapa 3: Mover arquivo apenas se todos os pedidos do arquivo foram enviados com sucesso
                    if (erro == 0)
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

        private MarketingDocuments MapOrdersFields(ListAttributeOrders pedido, int bpl)
        {

            var marketingDocument = new MarketingDocuments();

            var tst1 = pedido.Orders.FirstOrDefault().Quantidade.Substring(0, pedido.Orders.FirstOrDefault().Quantidade.Length - 3);

            var tst2 = pedido.Orders.FirstOrDefault().Quantidade.Substring(pedido.Orders.FirstOrDefault().Quantidade.Length - 3, 3);
            var tst = Convert.ToDouble($"{pedido.Orders.FirstOrDefault().Quantidade.Substring(0, pedido.Orders.FirstOrDefault().Quantidade.Length - 3)}.{pedido.Orders.FirstOrDefault().Quantidade.Substring(pedido.Orders.FirstOrDefault().Quantidade.Length - 3, 3)}",
                                        CultureInfo.InvariantCulture);
            marketingDocument = new MarketingDocuments
            {
                CardCode = pedido.Orders.FirstOrDefault()?.ClienteInterno,
                TaxDate = DateTime.ParseExact(pedido.Orders.FirstOrDefault()?.UE_DtHrEmissao, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                DocDueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                U_IdPrograma = pedido.Orders.FirstOrDefault()?.IdPrograma,
                BPL_IDAssignedToInvoice = bpl,
                DocumentLines = pedido.Orders.Select(x => new MarketingDocuments.Documentline
                {
                    ItemCode = x.ProdutoLocal,
                    Quantity =
                        Convert.ToDouble($"{x.Quantidade.Substring(0, x.Quantidade.Length - 3)}.{x.Quantidade.Substring(x.Quantidade.Length - 3, 3)}",
                                        CultureInfo.InvariantCulture),
                    VendorNum = x.ProdutoCliente,
                    SupplierCatNum = x.ProdutoCliente,
                    ShipDate = x.DtHrDE != null ? DateTime.ParseExact(x.DtHrDE, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd") : "",
                    U_Fabrica = x.Fabrica,
                    U_PV_PedC = x.PedidoNro,
                    U_Local_PE = x.Local_PE,
                    U_TipoFornec = x.TipoFornecimento,
                    U_CallDelivery = x.CallDelivery,
                    U_UE_Serie = x.UE_Serie,
                    U_Doca_PE = x.Doca_PE
                }).ToList()
            };

            return marketingDocument;
        }

        private async Task<QueryOrderExists> OrderExists(string pedidoNro, string tipodoc)
        {
            try
            {
                _logger.LogInformation($"Iniciando a busca dos Pedidos/Cotação {tipodoc} no SAP");

                string qr = string.Empty;
                if (tipodoc == "PE")
                {
                    qr = await SqlQueryLoader.LoadAsync("FindCotacaoVenda.sql");
                }
                else if (tipodoc == "PD")
                {
                    qr = await SqlQueryLoader.LoadAsync("FindPedidoVenda.sql");
                }
                else
                {
                    _logger.LogError($"Tipo de documento desconhecido: {tipodoc}");
                    return new QueryOrderExists();
                }

                var pedido = await _dbQueryExecutor.ExecuteQueryAsync<QueryOrderExists>(qr, pedidoNro);

                if (pedido == null || pedido.Count() == 0)
                {
                    _logger.LogInformation("Nenhum Pedido / Cotação de Vendas encontrado.");
                    return new QueryOrderExists();
                }

                return pedido.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos no SAP");
                return new QueryOrderExists();
            }
        }

        private async Task<bool> CreateDocument(ListAttributeOrders pedido, string path)
        {
            var bpl = Convert.ToInt32(AppConfig.Configuration["Filial"]);

            var sapObject = MapOrdersFields(pedido, bpl);
            var tipoDoc = pedido.TipoDoc == "PE" ? "CotacaoVendas" : "PedidoVendas";
            try
            {
                var response = await _serviceLayerClient.PostAsync<MarketingDocuments>(pedido.TipoDoc == "PE" ? "Quotations" : "Orders", sapObject);
                _logger.LogInformation($"Pedido {pedido.PedidoNro} enviado com sucesso!");
                NEXX_LOG log = new NEXX_LOG()
                {
                    NEXX_IdDoc = pedido.PedidoNro,
                    NEXX_TipoDoc = tipoDoc,
                    NEXX_Status = "2",
                    NEXX_MsgRet = $"{tipoDoc} criado no SAP com sucesso",
                    NEXX_IdDocLeg = response.DocNum.ToString(),
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
                    NEXX_TipoDoc = tipoDoc,
                    NEXX_Status = "3",
                    NEXX_MsgRet = $"Erro ao criar {tipoDoc} : {ex.Message}",
                    NEXX_JsonEnv = sapObject,
                    NEXX_JsonRet = ex.ToString(),
                    NEXX_IdRet = path
                };
                await log.InsertOrUpdateLog(_dbQueryExecutor);

                return false;
            }
        }

        private async Task<bool> UpdateDocument(ListAttributeOrders pedido, string path, string docEntry, string docNum)
        {
            var bpl = Convert.ToInt32(AppConfig.Configuration["Filial"]);

            var sapObject = new MarketingDocuments();
            var tipoDoc = pedido.TipoDoc == "PE" ? "CotacaoVendas" : "PedidoVendas";
            try
            {
                sapObject = MapOrdersFields(pedido, bpl);

                var endPoint = pedido.TipoDoc == "PE" ? "Quotations" : "Orders";
                await _serviceLayerClient.PatchAsync<MarketingDocuments>($"{endPoint}({docEntry})", sapObject, replaceCollectionsOnPatch: true);
                _logger.LogInformation($"Pedido {pedido.PedidoNro} atualizado com sucesso!");
                NEXX_LOG log = new NEXX_LOG()
                {
                    NEXX_IdDoc = pedido.PedidoNro,
                    NEXX_TipoDoc = tipoDoc,
                    NEXX_Status = "2",
                    NEXX_MsgRet = $"{tipoDoc} atualizado no SAP com sucesso",
                    NEXX_IdDocLeg = docNum,
                    NEXX_JsonEnv = sapObject,
                    NEXX_IdRet = path
                };
                await log.InsertOrUpdateLog(_dbQueryExecutor);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar pedido {pedido.PedidoNro}: {ex.Message}");

                NEXX_LOG log = new NEXX_LOG()
                {
                    NEXX_IdDoc = pedido.PedidoNro,
                    NEXX_TipoDoc = tipoDoc,
                    NEXX_Status = "3",
                    NEXX_MsgRet = $"Erro ao atualizar {tipoDoc} : {ex.Message}",
                    NEXX_JsonEnv = sapObject,
                    NEXX_JsonRet = ex.ToString(),
                    NEXX_IdRet = path
                };
                await log.InsertOrUpdateLog(_dbQueryExecutor);

                return false;
            }
        }

        public class ListAttributeOrders
        {
            public string PedidoNro { get; set; }
            public string TipoDoc { get; set; }
            public List<AttributeOrders> Orders { get; set; }
            public string FileName { get; set; }
        }

        public class QueryOrderExists
        {
            public int DocEntry { get; set; }
            public int DocNum { get; set; }
        }
    }
}
