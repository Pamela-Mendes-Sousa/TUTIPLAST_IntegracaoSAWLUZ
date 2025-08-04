using NEXX_SAWLUZIntegration.Utils;
using NEXX_SAWLUZIntegration.Models;
using NEXX_SAWLUZIntegration.Services;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Numerics;
using System.Reflection;
using Nexx.Core.Logging.Interfaces;
using Nexx.Core.ODBC.Query;
using Nexx.Core.ServiceLayer.Client;
using Nexx.Core.ServiceLayer.Setup.Interfaces;
using Nexx.Core.ODBC.Connections;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text.Json;
using Nexx.Core.ODBC.Helpers;
using static NEXX_SAWLUZIntegration.Services.MarketingDocumentsIntegration;

namespace NEXX_SAWLUZIntegration
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        

        public Worker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //criar os campos
            using (var scope = _serviceProvider.CreateScope())
            {
                var _logger = scope.ServiceProvider.GetRequiredService<ILogger<Worker>>();
                var _loggerMKT = scope.ServiceProvider.GetRequiredService<ILogger<MarketingDocumentsIntegration>>();

                var _dbQueryExecutor = scope.ServiceProvider.GetRequiredService<IDbQueryExecutor>();
                var _serviceLayerClient = scope.ServiceProvider.GetRequiredService<IServiceLayerClient>();
                var _marketingDocuments = scope.ServiceProvider.GetRequiredService<MarketingDocumentsIntegration>();

                #region Cria Campos e Tabelas
                var services = scope.ServiceProvider;
                var tableService = services.GetRequiredService<IIntegrationTableService>();
                var fieldService = services.GetRequiredService<IIntegrationFieldService>();
                var objectService = services.GetRequiredService<IIntegrationObjectService>();

                var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase\\JSON\\SMA_TablesFields.json");

                await tableService.CreateTablesAsync(schemaPath);
                await fieldService.CreateFieldsAsync(schemaPath);
                await objectService.CreateObjectsAsync(schemaPath);
                #endregion


                while (true)
                {
                    try
                    {
                        Console.WriteLine($@"Iniciando Rotina");
                        _logger.LogInformation("Iniciando Rotina");

                        //PEDIDO VENDAS
                        var pathIn = AppConfig.Configuration["PATH_IN"];
                        var pathLanc = AppConfig.Configuration["PATH_LANCADO"];

                        string[] inarqTxt = Directory.GetFiles(pathIn, $@"*.txt");

                        await _marketingDocuments.ProcessFileAsync(inarqTxt.ToList(), pathLanc);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Erro:" + ex.ToString());
                        _logger.LogError(ex, "Erro ao executar a rotina");
                    }
                    finally
                    {
                        Console.WriteLine("Serviço Finalizado");
                        _logger.LogInformation("Serviço Finalizado");
                        GC.Collect();
                        var sleepTime = TimeSpan.FromMinutes(Convert.ToInt32(AppConfig.Configuration["SleepTime"]));
                        Thread.Sleep(sleepTime);
                    }
                }
            }
        }
    }
}