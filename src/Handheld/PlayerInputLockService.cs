using Il2Cpp;

using CabinBoy.Core;
using CabinBoy.Emulator;
using CabinBoy.UI;

namespace CabinBoy.Handheld;


public static class PlayerInputLockService
{
    private static bool _isLocked;

    public static bool IsLocked() => _isLocked;


    public static void Update()
    {
        bool shouldLock =
            HandheldOverlayController.IsVisible()
            && UnityGbAdapter.GetState() == EmulatorState.Running
            && !BootScreenController.IsActive();

        if (shouldLock && !_isLocked)
            Lock();

        if (!shouldLock && _isLocked)
            Unlock();
    }


    private static void Lock()
    {
        var playerManager = GameManager.GetPlayerManagerComponent();

        if (playerManager == null)
        {
            ModLogger.Warning("PlayerInputLockService: PlayerManager not found.");
            return;
        }

        playerManager.SetControlMode(PlayerControlMode.Locked);
        _isLocked = true;

        ModLogger.Msg("Player input locked.");
    }

    private static void Unlock()
    {
        var playerManager = GameManager.GetPlayerManagerComponent();

        if (playerManager == null)
        {
            ModLogger.Warning("PlayerInputLockService: PlayerManager not found.");
            _isLocked = false;
            return;
        }

        playerManager.SetControlMode(PlayerControlMode.Normal);
        _isLocked = false;

        ModLogger.Msg("Player input unlocked.");
    }
}
