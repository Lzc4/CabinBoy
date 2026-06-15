using System;

using Il2Cpp;

using UnityEngine;

using CabinBoy.Config;
using CabinBoy.Core;

namespace CabinBoy.Handheld;


public static class HandheldInventoryService
{
    public static bool HasHandheld()
    {
        if (CabinBoyConfigManager.Config.DevModeAllowsHandheld)
            return true;

        return CheckScene();
    }

    public static bool CanUseHandheld()
    {
        if (!HasHandheld())
        {
            ModLogger.Warning(
                "Cannot open CabinBoy: missing item "
                + CabinBoyConfigManager.Config.RequiredHandheldGearName);

            return false;
        }

        return true;
    }

    public static bool IsDevModeEnabled()
        => CabinBoyConfigManager.Config.DevModeAllowsHandheld;


    private static bool CheckScene()
    {
        string required = CabinBoyConfigManager.Config.RequiredHandheldGearName;

        if (string.IsNullOrEmpty(required))
            return false;

        try
        {

            foreach (GearItem item in UnityEngine.Object.FindObjectsOfType<GearItem>())
            {
                if (item == null)
                    continue;

                string itemName = item.gameObject.name
                    .Replace("(Clone)", "")
                    .Trim();

                if (itemName == required)
                {
                    ModLogger.Msg("Found handheld item in scene: " + itemName);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.Error("Inventory check failed: " + ex);
        }

        ModLogger.Warning("Required handheld item not found.");
        return false;
    }
}
