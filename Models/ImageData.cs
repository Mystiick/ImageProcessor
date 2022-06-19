namespace ImageProcessor.Models;

public struct ImageData
{
    public string Image { get; init; }
    public string Thumbnail { get; init; }
    public DateTime CreatedDate { get; init; }
    public TagData TagData { get; init; }
    public CameraSettings Camera { get; init; }

    public ImageData()
    {
        Image = "";
        Thumbnail = "";
        CreatedDate = DateTime.Now;
        TagData = new TagData();
        Camera = new CameraSettings();
    }

    public ImageData(ImageData copyFrom)
    {
        Image = copyFrom.Image;
        Thumbnail = copyFrom.Thumbnail;
        CreatedDate = copyFrom.CreatedDate;
        TagData = copyFrom.TagData;
        Camera = copyFrom.Camera;
    }
}