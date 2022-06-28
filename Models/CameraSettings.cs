namespace ImageProcessor.Models;

public struct CameraSettings
{
    public string Model { get; init; }
    public ushort Flash { get; init; } // Need to map from 0/1/2/... to enum
    public uint ISO { get; init; }
    public string ShutterSpeed { get; init; }
    public string Aperature { get; init; }
    public string FocalLength { get; init; }
}