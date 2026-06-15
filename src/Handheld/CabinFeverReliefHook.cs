using Il2Cpp;

using CabinBoy.Core;

namespace CabinBoy.Handheld;

public static class CabinFeverReliefHook
{
    private static bool _pendingRelief;

    public static void RequestReliefTick()
    {
        _pendingRelief = true;
    }

    public static void TryApply(CabinFever cabinFever)
    {
        if (!_pendingRelief)
            return;

        if (cabinFever == null)
            return;

        cabinFever.ClearCabinFeverRisk();

        _pendingRelief = false;

        ModLogger.Msg("Applied real Cabin Fever relief.");
    }
}
