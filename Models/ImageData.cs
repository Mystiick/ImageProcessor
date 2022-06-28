namespace ImageProcessor.Models;

public struct ImageData
{
    public string GUID { get; init; }
    public string Image { get; init; }
    public string Thumbnail { get; init; }
    public string Preview { get; init; }
    public string Extension { get; init; }
    public DateTime CreatedDate { get; init; }
    public TagData TagData { get; init; }
    public CameraSettings Camera { get; init; }

    public ImageData()
    {
        GUID = "";
        Image = "";
        Thumbnail = "";
        Preview = "";
        Extension = "";
        CreatedDate = DateTime.Now;
        TagData = new TagData();
        Camera = new CameraSettings();
    }

    public ImageData(ImageData copyFrom)
    {
        GUID = copyFrom.GUID;
        Image = copyFrom.Image;
        Thumbnail = copyFrom.Thumbnail;
        Preview = copyFrom.Preview;
        Extension = copyFrom.Extension;
        CreatedDate = copyFrom.CreatedDate;
        TagData = copyFrom.TagData;
        Camera = copyFrom.Camera;
    }
}