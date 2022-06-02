using Microsoft.Extensions.Options;

using ImageProcessor.Models;

namespace ImageProcessor.Services;

public class ImageProcessorService
{
    private ImageClient _client;
    private FilePaths _paths;

    public ImageProcessorService(IOptions<FilePaths> options, ImageClient client)
    {
        _client = client;
        _paths = options.Value;
    }

    public async Task DoWork()
    {
        Console.WriteLine(_paths.RootFolder);
        await this.ProcessFolder();
    }


    public async Task ProcessFolder()
    {
        // If there are folders in the processing folder
        await _client.DoWork().ConfigureAwait(false);
    }
}