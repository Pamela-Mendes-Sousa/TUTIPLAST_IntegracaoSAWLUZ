using Nexx.Core.ServiceLayer.Setup.Interfaces;
using NEXX_SAWLUZIntegration;
using NEXX_SAWLUZIntegration.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
   .ConfigureServices((context, services) =>
   {
       services.AddHostedService<Worker>();
       services.AddNexxCore(context.Configuration); // Pass the required 'configuration' parameter  
       services.AddScoped<MarketingDocumentsIntegration>();


   })
   .Build();

System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

host.Run();
