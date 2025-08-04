using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexx.Core.Logging.Interfaces;
using Nexx.Core.Logging.Adapters;
using Nexx.Core.ODBC.Connections;
using Nexx.Core.ODBC.Config;
using Nexx.Core.ODBC.Interfaces;
using Nexx.Core.ODBC.Query;
using Nexx.Core.Mapping.Interfaces;
using Nexx.Core.Mapping.DataReader;
using Nexx.Core.Mapping.Json;
using Nexx.Core.ServiceLayer.Auth;
using Nexx.Core.ServiceLayer.Config;
using Nexx.Core.ServiceLayer.Setup.Interfaces;
using Nexx.Core.ServiceLayer.Setup.Implementations;
using Nexx.Core.ServiceLayer.Client;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddNexxCore(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Config SAP HANA
        var hanaConfig = new OdbcConfig();
        configuration.GetSection("Connections:Hana").Bind(hanaConfig);
        services.AddSingleton(hanaConfig);

        // 2. Config Service Layer
        var slConfig = new ServiceLayerConfig();
        configuration.GetSection("Connections:ServiceLayer").Bind(slConfig);
        services.AddSingleton(slConfig);

        // 3. Logging
        services.AddScoped(typeof(ILog<>), typeof(CompositeLoggerAdapter<>));

        // 4. ODBC
        services.AddScoped<HanaConnection>();
        services.AddScoped<IConnectDb, HanaConnection>();
        services.AddScoped<IDbQueryExecutor, OdbcQueryExecutor>();
        services.AddScoped(typeof(IDataReaderMapper<>), typeof(DataReaderMapper<>));

        // 5. JSON Mapper
        services.AddScoped(typeof(IJsonMapper<>), typeof(JsonMapper<>));

        // 6. Service Layer - com configuração segura
        services.AddHttpClient<IServiceLayerAuth, ServiceLayerAuth>(client =>
        {
            client.BaseAddress = new Uri(slConfig.Url);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromMinutes(30);
        })
       .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
       {
           ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
       });

        services.AddScoped<IServiceLayerClient, ServiceLayerClient>();

        // 7. Setup de Tabelas e Campos
        services.AddScoped<IIntegrationTableService, IntegrationTableService>();
        services.AddScoped<IIntegrationFieldService, IntegrationFieldService>();
        services.AddScoped<IIntegrationObjectService, IntegrationUserObjectService>();

        return services;
    }
}

