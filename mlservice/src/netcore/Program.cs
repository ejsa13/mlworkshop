using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using mlservice.Interface;
using Serilog;

namespace mlservice
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostContext, config) => {
                        config.AddJsonFile("appsettings.json");
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices((hostContext, services) => {
                        services.AddHttpClient();
                        services.AddSingleton(typeof(ILogger), new LoggerConfiguration()
                                                                    .ReadFrom.Configuration(hostContext.Configuration)
                                                                    .CreateLogger());
                        services.AddSingleton<IDetectorService, DetectorService>();
                        services.AddScoped<IKafkaHandler<string, string>, KafkaEventHandler>();
                        services.AddSingleton(typeof(IKafkaConsumer<,>), typeof(KafkaConsumer<,>));
                        services.AddHostedService<KafkaConsumerService>();
                    })
                    .RunConsoleAsync();
        }
    }
}
