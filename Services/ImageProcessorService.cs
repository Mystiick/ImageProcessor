using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Transactions;

using ImageProcessor.Models;

namespace ImageProcessor.Services;

public class ImageProcessorService
{
    private readonly ProcessorConfig _config;
    private readonly ILogger<ImageProcessorService> _logger;
    private readonly FileClient _fileClient;
    private readonly DataClient _dataClient;

    /// <summary>DI Constructor</summary>
    public ImageProcessorService(ILogger<ImageProcessorService> logger, IOptions<ProcessorConfig> options, FileClient fileClient, DataClient dataClient)
    {
        _fileClient = fileClient;
        _config = options.Value;
        _logger = logger;
        _dataClient = dataClient;
    }

    public async Task DoWork()
    {
        await this.ProcessDirectory(_config.SourceFolder);
    }

    public async Task ProcessDirectory(string directory)
    {
        // Get the images and tags
        List<string> images = _fileClient.GetImages(directory).ToList();
        TagData tags = await _fileClient.GetTags(directory);

        // Create the thumbnails
        List<ImageData> imageSource = new List<ImageData>();
        _logger.LogInformation($"Processing {directory}. {images.Count()} images found.");

        if (images.Count() > 0)
        {
            foreach (string img in images)
            {
                imageSource.Add(await _fileClient.CreateThumbnail(img, tags));
            }

            // Determine new paths so we can save their final locations to the database
            List<(ImageData source, ImageData destination)> archiveData = DetermineDestinations(imageSource);

            // 1 minute timeout
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 1, 0), TransactionScopeAsyncFlowOption.Enabled))
            {
                // Save data to DB
                await _dataClient.SaveImageData(archiveData.Select(x => x.destination).ToList());

                // Archive images and delete \tags
                _fileClient.ArchiveFiles(archiveData);

                scope.Complete();
            }
        }

        // Process subdirectories
        foreach (string sub in _fileClient.GetSubDirectories(directory))
        {
            await ProcessDirectory(sub);
        }

        _fileClient.DeleteDirectory(directory);
    }

    /// <summary>Returns a grouping of source and destination images, the destination images have their paths updated to the archive folder</summary>
    private List<(ImageData source, ImageData destination)> DetermineDestinations(List<ImageData> sourceFiles)
    {
        return sourceFiles
            .Select(source => (
                source,
                new ImageData(source)
                {
                    Image = source.Image.Replace(_config.SourceFolder, _config.ArchiveFolder),
                    Thumbnail = source.Thumbnail.Replace(_config.SourceFolder, _config.ArchiveFolder),
                }
            ))
            .ToList();
    }
}
