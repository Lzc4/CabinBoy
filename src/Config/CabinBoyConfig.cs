namespace CabinBoy.Config;

public class CabinBoyConfig
{

    public bool DevModeAllowsHandheld = false;


    public bool DebugOverlayEnabled = false;


    public string RequiredHandheldGearName = "GEAR_CabinBoy";


    public float CabinFeverReliefTickSeconds = 60f;


    public int ToggleKey = 297;

    public int CloseKey = 289;

    public bool EnableCabinFeverRelief = true;


    public float OverlayWidth  = 620f;
    public float OverlayHeight = 620f;


    public bool AutoLoadLastRom = true;


    public string LastLoadedRomPath = "";

    public float HeldModelDistance = 0.55f;

    public float FastForwardMultiplier = 4f;

    public float MasterVolume = 0.6f;
}
