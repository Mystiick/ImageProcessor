namespace ImageProcessor.Models;

public class ProcessorConfig
{
    public const string ConfigName = nameof(ProcessorConfig);
    public string SourceFolder { get; set; } = string.Empty;
    public string ArchiveFolder { get; set; } = string.Empty;
    public string FailedFolder { get; set; } = string.Empty;
    public string[] ImageExtensions { get; set; } = new string[0];
    public float ThumbnailMaxSize { get; set; } = float.NaN;
    public string AutoTagFileName { get; set; } = string.Empty;
    public bool DeleteAutoTagFile { get; set; } = false;
    public bool OverwriteFilesInArchive { get; set; } = false;
    public string DatabaseConnectionString { get; set; } = "";
}