using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MySql.Data.MySqlClient;

using ImageProcessor.Models;

namespace ImageProcessor.Services;

public class DataClient
{
    private readonly ILogger<DataClient> _logger;
    private readonly ProcessorConfig _config;

    /// <summary>DI Constructor</summary>
    public DataClient(ILogger<DataClient> logger, IOptions<ProcessorConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    public async Task SaveImageData(List<ImageData> images)
    {
        _logger.LogInformation($"Beginning database processing");
        using var connection = new MySqlConnection(_config.DatabaseConnectionString);
        await connection.OpenAsync();

        // TODO: Batch insert these instead of one at a time
        foreach (ImageData img in images)
        {
            int newID = await InsertImage(img, connection);

            await InsertCameraSettings(img, newID, connection);

            // TODO: Batch insert these instead of one at a time
            foreach (string tag in img.TagData.Tags)
            {
                await InsertTag(tag, newID, connection);
            }
        }

        _logger.LogInformation($"Done with database processing");
    }

    private async Task<int> InsertImage(ImageData img, MySqlConnection connection)
    {
        var command = new MySqlCommand("insert into Image (GUID, ImagePath, ThumbnailPath, PreviewPath, Category, SubCategory, Created) values (@GUID, @Image, @Thumbnail, @Preview, @Category, @SubCategory, @Created);", connection);

        command.Parameters.AddWithValue("@GUID", img.GUID);
        command.Parameters.AddWithValue("@Image", img.Image);
        command.Parameters.AddWithValue("@Thumbnail", img.Thumbnail);
        command.Parameters.AddWithValue("@Preview", img.Preview);
        command.Parameters.AddWithValue("@Category", img.TagData.Category);
        command.Parameters.AddWithValue("@Subcategory", img.TagData.SubCategory);
        command.Parameters.AddWithValue("@Created", img.CreatedDate);

        await command.PrepareAsync();
        await command.ExecuteNonQueryAsync();

        return int.Parse((await new MySqlCommand("select LAST_INSERT_ID() as 'LastID';", connection).ExecuteScalarAsync())?.ToString() ?? "");
    }

    private async Task InsertCameraSettings(ImageData img, int imageID, MySqlConnection connection)
    {
        var command = new MySqlCommand("insert into ImageSettings (ImageID, Model, Flash, ISO, ShutterSpeed, Aperature, FocalLength) values (@ImageID, @Model, @Flash, @ISO, @ShutterSpeed, @Aperature, @FocalLength);", connection);

        command.Parameters.AddWithValue("@ImageID", imageID);
        command.Parameters.AddWithValue("@Model", img.Camera.Model);
        command.Parameters.AddWithValue("@Flash", img.Camera.Flash);
        command.Parameters.AddWithValue("@ISO", img.Camera.ISO);
        command.Parameters.AddWithValue("@ShutterSpeed", img.Camera.ShutterSpeed);
        command.Parameters.AddWithValue("@Aperature", img.Camera.Aperature);
        command.Parameters.AddWithValue("@FocalLength", img.Camera.FocalLength);

        await command.PrepareAsync();
        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertTag(string tag, int imageID, MySqlConnection connection)
    {
        var tagCommand = new MySqlCommand("insert into ImageTag (ImageID, TagName) values (@ImageID, @TagName);", connection);
        tagCommand.Parameters.AddWithValue("@ImageID", imageID);
        tagCommand.Parameters.AddWithValue("@TagName", tag);

        await tagCommand.PrepareAsync();
        await tagCommand.ExecuteNonQueryAsync();
    }
}
