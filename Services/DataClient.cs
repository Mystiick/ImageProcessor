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
            var command = new MySqlCommand("insert into Image (GUID, ImagePath, ThumbnailPath, Category, SubCategory, CreatedDate) values (@GUID, @Image, @Thumbnail, @Category, @SubCategory, @Created);", connection);

            command.Parameters.AddWithValue("@GUID", Guid.NewGuid().ToString());
            command.Parameters.AddWithValue("@Image", img.Image);
            command.Parameters.AddWithValue("@Thumbnail", img.Thumbnail);
            command.Parameters.AddWithValue("@Category", img.MetaData.Category);
            command.Parameters.AddWithValue("@Subcategory", img.MetaData.SubCategory);
            command.Parameters.AddWithValue("@Created", img.CreatedDate);

            await command.PrepareAsync();
            await command.ExecuteNonQueryAsync();

            int newID = int.Parse((await new MySqlCommand("select LAST_INSERT_ID() as 'LastID';", connection).ExecuteScalarAsync())?.ToString() ?? "");

            // TODO: Batch insert these instead of one at a time
            foreach (string tag in img.MetaData.Tags)
            {
                var tagCommand = new MySqlCommand("insert into ImageTag (ImageID, TagName) values (@ImageID, @TagName);", connection);
                tagCommand.Parameters.AddWithValue("@ImageID", newID);
                tagCommand.Parameters.AddWithValue("@TagName", tag);

                await tagCommand.PrepareAsync();
                await tagCommand.ExecuteNonQueryAsync();
            }
        }

        _logger.LogInformation($"Done with database processing");
    }
}
