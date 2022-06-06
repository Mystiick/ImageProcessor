namespace ImageProcessor.Models;

public struct TagData
{
    public string[] Tags;
    public string Category;
    public string SubCategory;

    public TagData()
    {
        Category = "";
        SubCategory = "";
        Tags = new string[0];
    }
}