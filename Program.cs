using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ImageProcessor.Services;
using ImageProcessor.Models;

namespace ImageProcessor;

public class ImageProcessor
{
    public static async Task Main(string[] args)
    {
        using IHost host = CreateHostBuilder(args).Build();

        using IServiceScope scope = host.Services.CreateAsyncScope();

        var service = scope.ServiceProvider.GetRequiredService<ImageProcessorService>();

        await service.DoWork();
    }

    static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Add services
                services.AddScoped<ImageProcessorService>();
                services.AddScoped<FileClient>();

                // Add IOptions
                services.Configure<ProcessorConfig>(
                    context.Configuration.GetSection(ProcessorConfig.ConfigName)
                );
            })
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile("appsettings.json", optional: false);
#if DEBUG
                config.AddJsonFile("appsettings.dev.json", optional: true);
#endif
            })
            .ConfigureLogging(logging =>
            {
                logging.AddSimpleConsole(x =>
                {
                    x.SingleLine = true;
                    x.TimestampFormat = "yyyy-MM-dd hh:mm:ss tt ";
                    x.IncludeScopes = true;
                });
            })
        ;

        return builder;
    }
}
