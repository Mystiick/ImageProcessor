using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ImageProcessor.Models;

namespace ImageProcessor.Services;

public class DataClient
{
    private readonly ILogger<DataClient> _logger;
    private readonly ProcessorConfig _config;

    public DataClient(ILogger<DataClient> logger, IOptions<ProcessorConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    public async Task SaveImageData(List<ImageData> images)
    {
        _logger.LogInformation("DataClient called, but nothing happened");
        await Task.CompletedTask;
    }
}