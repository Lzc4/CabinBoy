using System.IO;
using MelonLoader;

namespace CabinBoy.Core;

public static class ModPaths
{
    public static readonly string ModFolder =
        Path.Combine("Mods", "CabinBoy");

    public static readonly string RomsFolder =
        Path.Combine(ModFolder, "Roms");

    public static readonly string SavesFolder =
        Path.Combine(ModFolder, "Saves");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(ModFolder);
        Directory.CreateDirectory(RomsFolder);
        Directory.CreateDirectory(SavesFolder);

        ModLogger.Msg("Directory check complete.");
    }
}
