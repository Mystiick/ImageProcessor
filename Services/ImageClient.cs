namespace ImageProcessor.Services;

public class ImageClient
{
    public ImageClient()
    {
    }

    public async Task DoWork()
    {
        Console.WriteLine("Client doing work");

        await Task.CompletedTask;
    }
}