using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SmbAbstraction;
using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace SmbAbstraction.Examples.DependencyInjection
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ISMBClientFactory>(new SMB2ClientFactory());
                    services.AddSingleton<ISMBCredentialProvider>(new SMBCredentialProvider());

                    
                    var serviceCollection = services.BuildServiceProvider();

                    var clientFactory = serviceCollection.GetRequiredService<ISMBClientFactory>();
                    var credentialProvider = serviceCollection.GetRequiredService<ISMBCredentialProvider>();
                    var loggerFactory = serviceCollection.GetRequiredService<ILoggerFactory>(); //optional

                    services.AddSingleton<IFileSystem>(new SMBFileSystem(clientFactory, credentialProvider, 65536u, loggerFactory));
                });
    }
}
