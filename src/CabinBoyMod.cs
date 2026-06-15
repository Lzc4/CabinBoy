using HarmonyLib;

using MelonLoader;

using UnityEngine;

using CabinBoy.Config;
using CabinBoy.Core;
using CabinBoy.Emulator;
using CabinBoy.Handheld;
using CabinBoy.Input;
using CabinBoy.UI;
using CabinBoy.World;

[assembly: MelonInfo(
    typeof(CabinBoy.CabinBoyMod),
    ModConstants.ModName,
    ModConstants.ModVersion,
    ModConstants.ModAuthor)]

[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace CabinBoy;

public class CabinBoyMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        ModLogger.Msg("Initializing CabinBoy...");

        ModPaths.EnsureDirectories();
        CabinBoyConfigManager.Load();
        RomLibrary.Refresh();
        HarmonyInstance.PatchAll();

        ModLogger.Msg("CabinBoy initialized successfully.");
    }

    public override void OnUpdate()
    {
        if (HotkeyHandler.IsTogglePressed())
        {
            HandheldOverlayController.Toggle();
            ModLogger.Msg("Toggled CabinBoy overlay.");
        }

        BootScreenController.Update(Time.deltaTime);
        HandheldOverlayController.HandleInput();
        UnityGbAdapter.Update();
        CabinFeverReliefService.Update(Time.deltaTime);
        PlayerInputLockService.Update();
        CabinBoyHeldModelService.Update(HandheldOverlayController.IsVisible());
    }

    public override void OnGUI()
    {
        HandheldOverlayController.Draw();
    }
}
