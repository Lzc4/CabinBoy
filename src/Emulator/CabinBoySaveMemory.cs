using System.IO;

using UnityGB;

using CabinBoy.Core;

namespace CabinBoy.Emulator;

public class CabinBoySaveMemory : ISaveMemory
{
    public void Save(string name, byte[] data)
    {
        string path = GetSavePath(name);

        File.WriteAllBytes(path, data);

        ModLogger.Msg("Saved SRAM: " + path);
    }

    public byte[] Load(string name)
    {
        string path = GetSavePath(name);

        if (!File.Exists(path))
        {
            ModLogger.Msg("No SRAM found for: " + name);
            return null;
        }

        ModLogger.Msg("Loaded SRAM: " + path);

        return File.ReadAllBytes(path);
    }

    private string GetSavePath(string name)
    {
        return Path.Combine(
            ModPaths.SavesFolder,
            name + ".sav");
    }
}
