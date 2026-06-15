using System.IO;

namespace CabinBoy.Emulator;

public class RomInfo
{
    public string FileName;

    public string FullPath;

    public string Extension;

    public string DisplayTitle;

    public string CartridgeLabel;

    public RomInfo(string fileName, string fullPath)
    {
        FileName = fileName;
        FullPath = fullPath;
        Extension = Path.GetExtension(fullPath).ToLowerInvariant();

        DisplayTitle = Path.GetFileNameWithoutExtension(fileName);
        CartridgeLabel = BuildCartridgeLabel(DisplayTitle);
    }

    public RomInfo(string fileName, string fullPath, string extension)
    {
        FileName = fileName;
        FullPath = fullPath;
        Extension = extension;

        DisplayTitle = Path.GetFileNameWithoutExtension(fileName);
        CartridgeLabel = BuildCartridgeLabel(DisplayTitle);
    }

    private static string BuildCartridgeLabel(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return "UNKNOWN";

        string cleaned =
            raw.Replace("_", " ")
               .Replace("-", " ")
               .Trim();

        if (cleaned.Length > 24)
        {
            cleaned = cleaned.Substring(0, 24);
        }

        return cleaned.ToUpperInvariant();
    }
}
