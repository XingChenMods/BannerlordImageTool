using CsvHelper;
using CsvHelper.Configuration;
using ImageMagick;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace BannerlordImageTool.Sprite;

public class SpriteUnpacker
{
    public void UnpackSingle(string spriteSheet, string outputFile, SpriteRegion region)
    {
        if (string.IsNullOrEmpty(outputFile)) throw new ArgumentNullException("outputFile");
        var settings = new MagickReadSettings() { ExtractArea = region.ToGeometry() };
        using (var sprite = new MagickImage(spriteSheet, settings))
        {
            sprite.Write(outputFile);
        }
    }
    public void UnpackFromCSV(string csvFile, string sourceDir, string outputDir, string srcExt, string outExt)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture);
        using var reader = File.OpenText(csvFile);
        using var csv = new CsvReader(reader, config);
        var rows = csv.GetRecords<SpriteInfo>();
        foreach (var row in rows)
        {
            var sourceFile = Path.Combine(sourceDir, $"{row.Atlas}.{srcExt}");
            var outputFile = Path.Combine(outputDir, row.Atlas, $"{row.ID}.{outExt}");
            var dir = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            UnpackSingle(sourceFile, outputFile, row.Region);
        }
    }
}
public record SpriteRegion(int X, int Y, int Width, int Height)
{
    public static SpriteRegion FromString(string args)
    {
        var parts = args.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            throw new ArgumentException("invalid sprite region. should be in the format of x,y,width,height");
        }
        if (!int.TryParse(parts[0], out var x))
        {
            throw new ArgumentException($"invalid sprite x: {parts[0]}");
        }
        if (!int.TryParse(parts[1], out var y))
        {
            throw new ArgumentException($"invalid sprite y: {parts[1]}");
        }
        if (!int.TryParse(parts[2], out var w))
        {
            throw new ArgumentException($"invalid sprite w: {parts[2]}");
        }
        if (!int.TryParse(parts[3], out var h))
        {
            throw new ArgumentException($"invalid sprite h: {parts[3]}");
        }
        return new SpriteRegion(x, y, w, h);
    }
    public MagickGeometry ToGeometry() => new MagickGeometry(X, Y, Width, Height);
}
public class SpriteInfo
{
    public string Atlas { get; set; } = "";
    public string ID { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public bool IsValid
    {
        get => !string.IsNullOrEmpty(Atlas)
            && !string.IsNullOrEmpty(ID)
            && Width > 0
            && Height > 0
            && X >= 0
            && Y >= 0;
    }

    public SpriteRegion Region
    {
        get => new(X, Y, Width, Height);
    }
}