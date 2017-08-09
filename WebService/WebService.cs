
namespace WebService
{
    using Common;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using System.Collections.Generic;
    using System.Fabric;
    using System.IO;

    internal sealed class WebService : StatelessService
    {
        public WebService(StatelessServiceContext serviceContext)
            : base(serviceContext)
        {
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(context =>
                {
                    ConfigurationPackage configPackage =
                        context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

                    string appPath = configPackage.Settings.Sections["Web"].Parameters["AppPath"].Value;
                    
                    return new WebHostCommunicationListener(context, appPath, "ServiceEndpoint", uri =>
                        new WebHostBuilder().UseWebListener()
                                           .UseContentRoot(Directory.GetCurrentDirectory())
                                           .ConfigureServices(services => services
                                                .AddSingleton<FabricClient>(new FabricClient())
                                                .AddSingleton<StatelessServiceContext>(context))
                                           .UseStartup<Startup>()
                                           .UseUrls(uri)
                                           .Build());
                })                         
            };
        }
    }
}
