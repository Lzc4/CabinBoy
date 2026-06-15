using System.IO;
using System.Text.Json;

using CabinBoy.Core;

namespace CabinBoy.Config;

public static class CabinBoyConfigManager
{
    private static readonly string ConfigPath =
        Path.Combine(ModPaths.ModFolder, "config.json");

    private static readonly JsonSerializerOptions JsonOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
        };

    private static CabinBoyConfig _config = new CabinBoyConfig();

    public static CabinBoyConfig Config => _config;


    public static void Load()
    {
        try
        {
            ModPaths.EnsureDirectories();

            if (!File.Exists(ConfigPath))
            {
                Save();
                ModLogger.Msg("Created default config at: " + ConfigPath);
                return;
            }

            string json = File.ReadAllText(ConfigPath);

            CabinBoyConfig loaded =
                JsonSerializer.Deserialize<CabinBoyConfig>(json, JsonOptions);

            if (loaded == null)
            {
                ModLogger.Warning("Config empty or invalid — using defaults.");
                _config = new CabinBoyConfig();
                Save();
                return;
            }

            _config = loaded;

            ModLogger.Msg("Config loaded. DevMode=" + _config.DevModeAllowsHandheld);
        }
        catch (System.Exception ex)
        {
            ModLogger.Error("Failed to load config: " + ex);
            _config = new CabinBoyConfig();
        }
    }

    public static void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(_config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch (System.Exception ex)
        {
            ModLogger.Error("Failed to save config: " + ex);
        }
    }
}

