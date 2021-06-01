using System.Threading.Tasks;
using ABKTest.Options;
using ABKTest.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ABKTest
{
    internal static class Program
    {
        private static async Task Main()
        {
            var configuration = LoadConfiguration();

            ConfigureLogger(configuration);

            var serviceProvider = ConfigureServices(configuration)
                .BuildServiceProvider();

            var application = serviceProvider.GetRequiredService<IApplication>();

            await application.Run();
        }

        private static void ConfigureLogger(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(x => x.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static IServiceCollection ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            services.AddLogging(configure => configure.AddSerilog());

            services.Configure<ApplicationOptions>(configuration.GetSection("Application"));

            services.AddTransient<IApplication, Application>();
            services.AddTransient<IEndpointBalancerService, EndpointBalancerService>();

            return services;
        }

        private static IConfiguration LoadConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            return configuration;
        }
    }
}
