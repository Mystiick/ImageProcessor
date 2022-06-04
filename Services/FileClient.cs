using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

using ImageProcessor.Models;

namespace ImageProcessor.Services;

/// <summary>Class responsible for the folder and file manipulation</summary>
public class FileClient
{

    private readonly ProcessorConfig _config;
    private readonly ILogger<FileClient> _logger;

    /// <summary>DI Constructor</summary>
    public FileClient(ILogger<FileClient> logger, IOptions<ProcessorConfig> config)
    {
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>Returns an enumerable of images in the specified <see cref="directory"/></summary>
    public IEnumerable<string> GetImages(string directory)
    {
        return Directory.EnumerateFiles(directory).Where(x =>
        {
            // Only return files with the extension specified in appsettings. 
            // Exclude any files with "_thumb." in their name to prevent processing thumbnails
            var fi = new FileInfo(x);
            return _config.ImageExtensions.Contains(fi.Extension) && !fi.Name.Contains("_thumb.");
        });
    }

    /// <summary>Returns an enumerable of subfolders that contain images</summary>
    public IEnumerable<string> GetSubDirectories(string directory)
    {
        return Directory.EnumerateDirectories(directory).Where(x => this.GetImages(x).Any());
    }

    /// <summary>
    /// Looks for a "tags" file in the specified directory and parses it out if one exists. If no file exists, it returns an empty TagData object with the category = folder name
    /// The data found in the tags file will apply to all images within in the current folder excluding any subfolders.
    /// </summary>
    /// <remarks>
    /// The tags file consists of 3 lines (excluding the counters below):
    /// 0: Category name
    /// 1: Subcategory Name
    /// 2: Comma, delimited, list, of tags
    /// </remarks>
    public async Task<TagData> GetTags(string directory)
    {
        var tagFile = Path.Combine(directory, "tags");

        if (File.Exists(tagFile))
        {
            // Read and parse the tags file
            string[] tags = await File.ReadAllLinesAsync(tagFile);

            return new TagData()
            {
                Category = tags.ElementAtOrDefault(0) ?? "",
                SubCategory = tags.ElementAtOrDefault(1) ?? "",
                Tags = (tags.ElementAtOrDefault(2) ?? "").Split(",").Select(x => x.Trim()).ToArray(),
            };
        }
        else
        {
            _logger.LogWarning("No tags file found, using folder name as category.");
            _logger.LogInformation($"Expected file \"{tagFile}\"");

            var di = new DirectoryInfo(directory);

            return new TagData()
            {
                Category = di.Name
            };
        }
    }

    /// <summary>Loads the <see cref="file"/> using ImageSharp, generates a thumbnail image</summary>
    public async Task<ImageData> CreateThumbnail(string file, TagData tags)
    {
        // Load the image
        var fi = new FileInfo(file);
        var img = await Image.LoadAsync(file);

        // Name the thumbnail
        string thumbnail = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + "_thumb" + fi.Extension;

        // Resize the image to be a max of 250 width and height, retaining the aspect ratio
        float ratio = (img.Width > img.Height ? img.Width : img.Height) / _config.ThumbnailMaxSize;
        img.Mutate(x => x.Resize((int)(img.Width / ratio), (int)(img.Height / ratio)));

        // Save the thumbnail
        await img.SaveAsync(thumbnail);

        return new ImageData()
        {
            Image = file,
            Thumbnail = thumbnail,
            CreatedDate = fi.CreationTime,
            Tags = tags
        };
    }
}
