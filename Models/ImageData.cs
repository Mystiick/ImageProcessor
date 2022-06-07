namespace ImageProcessor.Models;

public struct ImageData
{
    public string Image { get; init; }
    public string Thumbnail { get; init; }
    public DateTime CreatedDate { get; init; }
    public TagData MetaData { get; init; }

    public ImageData()
    {
        Image = "";
        Thumbnail = "";
        CreatedDate = DateTime.Now;
        MetaData = new TagData();
    }

    public ImageData(ImageData copyFrom)
    {
        Image = copyFrom.Image;
        Thumbnail = copyFrom.Thumbnail;
        CreatedDate = copyFrom.CreatedDate;
        MetaData = copyFrom.MetaData;
    }
}