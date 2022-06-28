using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

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
            return _config.ImageExtensions.Contains(fi.Extension.ToLower()) && !fi.Name.Contains("_thumb.") && !fi.Name.Contains("_preview.");
        });
    }

    /// <summary>Returns an enumerable of subfolders that contain images</summary>
    public IEnumerable<string> GetSubDirectories(string directory)
    {
        return Directory.EnumerateDirectories(directory);
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
        var tagFile = Path.Combine(directory, _config.AutoTagFileName);

        if (File.Exists(tagFile))
        {
            // Read and parse the tags file
            string[] tags = await File.ReadAllLinesAsync(tagFile);

            return new TagData()
            {
                Category = tags.ElementAtOrDefault(0) ?? "",
                SubCategory = tags.ElementAtOrDefault(1) ?? "",
                Tags = (tags.ElementAtOrDefault(2) ?? "").Split(",").Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray(),
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

    /// <summary>Loads the <see cref="file"/> using ImageSharp, generates a thumbnail image and a preview image to reduce total filesize going over the wire</summary>
    public async Task<ImageData> CreateThumbnail(string file, TagData tags)
    {
        // Load the image
        var fi = new FileInfo(file);
        var img = await Image.LoadAsync(file);
        var id = Guid.NewGuid().ToString();

        // Name the resized images
        string thumbnail = Path.Combine(fi.Directory?.FullName ?? "", id + "_thumb" + fi.Extension);
        string preview = Path.Combine(fi.Directory?.FullName ?? "", id + "_preview" + fi.Extension);

        // Resize the image to be a max of 250 width and height, retaining the aspect ratio
        float ratio = (img.Width > img.Height ? img.Width : img.Height) / _config.PreviewMaxSize;
        img.Mutate(x => x.Resize((int)(img.Width / ratio), (int)(img.Height / ratio)));

        // Save the preview
        await img.SaveAsync(preview);

        // Process thumbnail 
        ratio = (img.Width > img.Height ? img.Width : img.Height) / _config.ThumbnailMaxSize;
        img.Mutate(x => x.Resize((int)(img.Width / ratio), (int)(img.Height / ratio)));

        // Save the thumbnail
        await img.SaveAsync(thumbnail);

        return new ImageData()
        {
            GUID = Guid.NewGuid().ToString(),
            Image = file,
            Thumbnail = thumbnail,
            Preview = preview,
            Extension = fi.Extension,
            CreatedDate = DateTime.ParseExact(img.Metadata.ExifProfile.GetValue<string>(ExifTag.DateTime).ToString() ?? "", "yyyy:MM:dd HH:mm:ss", null),
            TagData = tags,
            Camera = new CameraSettings()
            {
                Model = img.Metadata.ExifProfile.GetValue<string>(ExifTag.Model).Value,
                Flash = img.Metadata.ExifProfile.GetValue<ushort>(ExifTag.Flash).Value,
                ISO = img.Metadata.ExifProfile.GetValue<uint>(ExifTag.RecommendedExposureIndex).Value,
                ShutterSpeed = SimplifyRational(img.Metadata.ExifProfile.GetValue<SixLabors.ImageSharp.Rational>(ExifTag.ExposureTime).Value),
                Aperature = SimplifyRational(img.Metadata.ExifProfile.GetValue<SixLabors.ImageSharp.Rational>(ExifTag.FNumber).Value),
                FocalLength = SimplifyRational(img.Metadata.ExifProfile.GetValue<SixLabors.ImageSharp.Rational>(ExifTag.FocalLength).Value)
            }
        };
    }

    /// <summary>
    /// Moves files from the source to the destination based on <see cref="input"/>. 
    /// If successful, it wil delete the AutoTag file if that is enbaled.
    /// </summary>
    public void ArchiveFiles(List<(ImageData source, ImageData destination)> input)
    {
        string sourceDir = Directory.GetParent(input[0].source.Image)?.FullName ?? "";
        string archiveDir = Directory.GetParent(input[0].destination.Image)?.FullName ?? "";

        // First validate all files are valid, so we don't move half of them and fail
        if (input.All(x => File.Exists(x.source.Image) && File.Exists(x.source.Thumbnail) && File.Exists(x.source.Preview)))
        {
            // Create a subfolder if needed.
            if (!Directory.Exists(archiveDir))
            {
                Directory.CreateDirectory(archiveDir);
            }

            // Once we know they are all valid, try to move them all
            foreach ((ImageData src, ImageData dest) in input)
            {
                _logger.LogInformation($"Moving {src.Image} to {dest.Image}");
                File.Move(src.Image, dest.Image, _config.OverwriteFilesInArchive);

                _logger.LogInformation($"Moving {src.Preview} to {dest.Preview}");
                File.Move(src.Preview, dest.Preview, _config.OverwriteFilesInArchive);

                _logger.LogInformation($"Moving {src.Thumbnail} to {dest.Thumbnail}");
                File.Move(src.Thumbnail, dest.Thumbnail, _config.OverwriteFilesInArchive);
            }

            // And now delete the /tags file once the copy has been successful
            if (File.Exists(Path.Combine(sourceDir, _config.AutoTagFileName)))
            {
                if (_config.DeleteAutoTagFile)
                {
                    File.Delete(
                        Path.Combine(sourceDir, _config.AutoTagFileName)
                    );
                }
                else
                {
                    File.Move(
                        Path.Combine(sourceDir, _config.AutoTagFileName),
                         Path.Combine(archiveDir, _config.AutoTagFileName + DateTime.Now.ToString("yyyyMMdd_HHmmss"))
                    );
                }
            }
        }
        else
        {
            // Something went wrong, and the files that were passed in could not be found. 
            // Throw an exception right away instead of copying half of them over and failing out, leaving a mess
            var missingFiles = input.Where(x => (!File.Exists(x.source.Image))).Select(x => x.source.Image)
                                    .Concat(
                                        input.Where(x => !File.Exists(x.source.Thumbnail)).Select(x => x.source.Thumbnail)
                                    );

            throw new FileNotFoundException($"The following files could not be found: \r\n{string.Join("\r\n", missingFiles)}");
        }
    }

    /// <summary>Deletes the specified folder if it's empty</summary>
    public void DeleteDirectory(string directory)
    {
        // Delete the folder if it's empty. This will throw an exception if the directory isn't actually empty, but that shouldn't happen
        if (directory != _config.SourceFolder && !Directory.GetFileSystemEntries(directory).Any())
        {
            _logger.LogInformation($"Deleting source folder {directory}");
            Directory.Delete(directory);
        }
    }

    /// <summary>
    /// Formats a ImageSharp.Rational into a more viewable string. Changes "3000/10" to "300" and "10/2000" to "1/200"
    /// </summary>
    public string SimplifyRational(Rational input)
    {
        if (input.Denominator > input.Numerator)
        {
            return $"1/{input.Denominator / input.Numerator}";
        }
        else
        {
            return $"{(float)input.Numerator / (float)input.Denominator}";

        }
    }
}
