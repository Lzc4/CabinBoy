using UnityEngine;

using CabinBoy.Config;

namespace CabinBoy.Input;

public static class HotkeyHandler
{
    public static bool IsTogglePressed()
    {
        KeyCode toggleKey =
            (KeyCode)CabinBoyConfigManager.Config.ToggleKey;

        return UnityEngine.Input.GetKeyDown(toggleKey);
    }
}
