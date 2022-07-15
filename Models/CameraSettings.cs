using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace ImageProcessor.Models;

public struct CameraSettings
{
    public string Model { get; init; }
    public ushort Flash { get; init; } // Need to map from 0/1/2/... to enum
    public uint ISO { get; init; }
    public string ShutterSpeed { get; init; }
    public string Aperature { get; init; }
    public string FocalLength { get; init; }

    public CameraSettings()
    {
        Model = string.Empty;
        Flash = 0;
        ISO = 0;
        ShutterSpeed = string.Empty;
        Aperature = string.Empty;
        FocalLength = string.Empty;
    }

    public CameraSettings(ExifProfile? exif) : this()
    {
        if (exif != null)
        {
            Model = exif.GetValue<string>(ExifTag.Model).Value;
            Flash = exif.GetValue<ushort>(ExifTag.Flash).Value;
            ISO = exif.GetValue<uint>(ExifTag.RecommendedExposureIndex).Value;
            ShutterSpeed = SimplifyRational(exif.GetValue<Rational>(ExifTag.ExposureTime).Value);
            Aperature = SimplifyRational(exif.GetValue<Rational>(ExifTag.FNumber).Value);
            FocalLength = SimplifyRational(exif.GetValue<Rational>(ExifTag.FocalLength).Value);
        }
    }

    /// <summary>
    /// Formats a ImageSharp.Rational into a more viewable string. Changes "3000/10" to "300" and "10/2000" to "1/200"
    /// </summary>
    public static string SimplifyRational(Rational input)
    {
        if (input.Denominator > input.Numerator)
        {
            return $"1/{input.Denominator / input.Numerator}";
        }
        else
        {
            return $"{(float)input.Numerator / (float)input.Denominator}";

        }
    }
}