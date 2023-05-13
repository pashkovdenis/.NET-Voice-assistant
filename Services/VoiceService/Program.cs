using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VoiceService.Extensions;
using VoiceService.Services;

namespace VoiceService
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                          .AddJsonFile("appsettings.json")
                          .AddEnvironmentVariables()
                          .Build();
             
            // Build host
            using IHost host = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
                { 
                    services.AddLogging();  
                    services.RegisterVoiceServices(config);
                    services.AddHostedService<VoiceHostedService>();
                    services.AddHostedService<TimerService>();  
                })
                .Build();
             
            await host.RunAsync();
        }
    }
}