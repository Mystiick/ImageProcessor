using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using ImageProcessor.Models;

namespace ImageProcessor.Services;

public class ImageProcessorService
{
    private readonly FileClient _fileClient;
    private readonly ProcessorConfig _config;
    private readonly ILogger<ImageProcessorService> _logger;

    public ImageProcessorService(ILogger<ImageProcessorService> logger, IOptions<ProcessorConfig> options, FileClient fileClient)
    {
        _fileClient = fileClient;
        _config = options.Value;
        _logger = logger;
    }

    public async Task DoWork()
    {
        await this.ProcessDirectory(_config.ProcessingFolder);
    }


    public async Task ProcessDirectory(string directory)
    {

        // Get the images and tags
        List<string> images = _fileClient.GetImages(directory).ToList();
        TagData tags = await _fileClient.GetTags(directory);

        // Create the thumbnails
        List<ImageData> imageSource = new List<ImageData>();
        _logger.LogInformation($"Processing {directory}. {images.Count()} images found.");
        foreach (string img in images)
        {
            imageSource.Add(await _fileClient.CreateThumbnail(img, tags));
        }

        // Determine new paths so we can save their final locations to the database


        // Save data to DB


        // Archive images and delete \tags


        // Process subdirectories
        foreach (string sub in _fileClient.GetSubDirectories(directory))
        {
            await ProcessDirectory(sub);
        }
    }
}