using HarmonyLib;

using Il2Cpp;

using CabinBoy.Handheld;

namespace CabinBoy.Patches;

[HarmonyPatch(typeof(CabinFever))]
public static class CabinFeverPatches
{
    [HarmonyPatch(nameof(CabinFever.Update))]
    [HarmonyPrefix]
    public static void UpdatePrefix(CabinFever __instance)
    {
        CabinFeverReliefHook.TryApply(__instance);
    }
}
