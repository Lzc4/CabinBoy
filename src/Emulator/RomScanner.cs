using System.Collections.Generic;
using System.IO;

using CabinBoy.Core;

namespace CabinBoy.Emulator;

public static class RomScanner
{
    public static List<RomInfo> ScanRoms()
    {
        List<RomInfo> roms = new List<RomInfo>();

        if (!Directory.Exists(ModPaths.RomsFolder))
        {
            ModLogger.Warning("ROM folder does not exist.");
            return roms;
        }

        AddRomsByExtension(roms, "*.gb");
        AddRomsByExtension(roms, "*.gbc");

        ModLogger.Msg($"Total ROMs found: {roms.Count}");

        return roms;
    }

    private static void AddRomsByExtension(List<RomInfo> roms, string searchPattern)
    {
        string[] files = Directory.GetFiles(
            ModPaths.RomsFolder,
            searchPattern,
            SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string extension = Path.GetExtension(file).ToLowerInvariant();

            roms.Add(new RomInfo(fileName, file, extension));

            ModLogger.Msg($"Found ROM: {fileName}");
        }
    }
}
