namespace ImageProcessor.Models;

public class ProcessorConfig
{
    public const string ConfigName = nameof(ProcessorConfig);
    public string ProcessingFolder { get; set; } = string.Empty;
    public string FailedFolder { get; set; } = string.Empty;
    public string[] ImageExtensions { get; set; } = new string[0];
    public float ThumbnailMaxSize { get; set; } = 0f;
}