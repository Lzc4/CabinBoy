using CabinBoy.Config;
using CabinBoy.Core;
using CabinBoy.Emulator;

namespace CabinBoy.Handheld;

public static class CabinFeverReliefService
{
    private static float _elapsedSeconds;

    public static void Update(float deltaTime)
    {
        if (!CabinBoyConfigManager.Config.EnableCabinFeverRelief)
            return;

        if (UnityGbAdapter.GetState() != EmulatorState.Running)
            return;

        _elapsedSeconds += deltaTime;

        HandheldUsageStats.AddSessionTime(deltaTime);

        if (_elapsedSeconds < CabinBoyConfigManager.Config.CabinFeverReliefTickSeconds)
            return;

        _elapsedSeconds = 0f;

        ApplyCabinFeverRelief();
    }

    private static void ApplyCabinFeverRelief()
    {
        HandheldUsageStats.AddReliefTick();

        CabinFeverReliefHook.RequestReliefTick();

        ModLogger.Msg(
            "Cabin fever relief tick requested. Total ticks: "
            + HandheldUsageStats.ReliefTicks);
    }

    public static void Reset()
    {
        _elapsedSeconds = 0f;
        HandheldUsageStats.ResetSession();
    }
}
