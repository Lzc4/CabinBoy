using MelonLoader;

namespace CabinBoy.Core;

public static class ModLogger
{
    public static void Msg(string message)
    {
        MelonLogger.Msg($"[CabinBoy] {message}");
    }

    public static void Warning(string message)
    {
        MelonLogger.Warning($"[CabinBoy] {message}");
    }

    public static void Error(string message)
    {
        MelonLogger.Error($"[CabinBoy] {message}");
    }
}
