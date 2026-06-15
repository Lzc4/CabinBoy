using HarmonyLib;

using Il2Cpp;

using CabinBoy.Config;
using CabinBoy.Core;
using CabinBoy.UI;

namespace CabinBoy.Patches;

public static class GearItemInteractionPatches
{
    private static string GetGearName(GearItem item)
    {
        try { return item.gameObject.name.Replace("(Clone)", "").Trim(); }
        catch { return null; }
    }

    internal static bool IsCabinBoy(GearItem item)
    {
        if (item == null) return false;
        return GetGearName(item) == CabinBoyConfigManager.Config.RequiredHandheldGearName;
    }
}

[HarmonyPatch(typeof(PlayerManager), "EquipItem")]
public static class PlayerManager_EquipItemPatch
{
    public static bool Prefix(GearItem gi)
    {
        if (!GearItemInteractionPatches.IsCabinBoy(gi))
            return true;

        HandheldOverlayController.RequestOpen();

        try
        {
            InterfaceManager.QuitCurrentScreens();
        }
        catch (System.Exception ex)
        {
            ModLogger.Warning("Could not close current screens: " + ex.Message);
        }

        return false;
    }
}

