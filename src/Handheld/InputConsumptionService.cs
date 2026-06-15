using UnityEngine;

using CabinBoy.Emulator;
using CabinBoy.UI;

namespace CabinBoy.Handheld;

public static class InputConsumptionService
{
    public static bool ShouldConsumeGameplayInput()
    {
        return HandheldOverlayController.IsVisible()
            && UnityGbAdapter.GetState() == EmulatorState.Running;
    }

    public static bool IsBlockedKey(KeyCode key)
    {
        if (!ShouldConsumeGameplayInput())
            return false;

        switch (key)
        {
            case KeyCode.W:
            case KeyCode.A:
            case KeyCode.S:
            case KeyCode.D:

            case KeyCode.J:
            case KeyCode.K:

            case KeyCode.Return:
            case KeyCode.RightShift:

            case KeyCode.UpArrow:
            case KeyCode.DownArrow:
            case KeyCode.LeftArrow:
            case KeyCode.RightArrow:

                return true;
        }

        return false;
    }
}
