using System.Collections.Generic;

namespace CabinBoy.Emulator;

public static class RomLibrary
{
    private static readonly List<RomInfo> Roms = new List<RomInfo>();

    public static IReadOnlyList<RomInfo> GetRoms()
    {
        return Roms;
    }

    public static void Refresh()
    {
        Roms.Clear();
        Roms.AddRange(RomScanner.ScanRoms());
    }

    public static RomInfo FindByFullPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return null;

        for (int i = 0; i < Roms.Count; i++)
        {
            RomInfo rom = Roms[i];

            if (rom.FullPath == fullPath)
                return rom;
        }

        return null;
    }
}
