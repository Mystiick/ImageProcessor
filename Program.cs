using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

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
                services.AddScoped<ImageClient>();

                // Add IOptions
                services.Configure<FilePaths>(
                    context.Configuration.GetSection(FilePaths.ConfigName)
                );
            })
            .ConfigureAppConfiguration(x =>
            {
                x.AddJsonFile("appsettings.json", optional: false);
            })
        ;

        return builder;
    }
}
