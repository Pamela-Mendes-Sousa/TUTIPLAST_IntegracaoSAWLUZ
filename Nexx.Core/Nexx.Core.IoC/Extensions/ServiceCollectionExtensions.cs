using Microsoft.Extensions.DependencyInjection;
using Nexx.Core.ServiceLayer.Client;
using Nexx.Core.ServiceLayer.Setup.Implementations;
using Nexx.Core.ServiceLayer.Setup.Interfaces;

namespace Nexx.Core.IoC.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIntegrationSetup(this IServiceCollection services)
        {
            //services.AddScoped<ITableSetupService, ITableSetupService>();
            //services.AddScoped<IFieldSetupService, IFieldSetupService>();
            services.AddHttpClient<IServiceLayerClient, ServiceLayerClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    });

            return services;
        }
    }
}
