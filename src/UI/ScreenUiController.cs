using UnityEngine;

using CabinBoy.Emulator;
using CabinBoy.Core;

namespace CabinBoy.UI;


public static class ScreenUiController
{
    private static readonly Color32 GbDarkest  = new Color32(0x0F, 0x38, 0x0F, 0xFF);
    private static readonly Color32 GbDark     = new Color32(0x30, 0x62, 0x30, 0xFF);
    private static readonly Color32 GbLight    = new Color32(0x8B, 0xAC, 0x0F, 0xFF);
    private static readonly Color32 GbLightest = new Color32(0x9B, 0xBC, 0x0F, 0xFF);


    private static int  _selectedIndex = 0;
    private static int  _scrollOffset  = 0;
    private static bool _pendingStop   = false;

    private const int VisibleRomSlots = 8;

    private static readonly GameBoyScreenRenderer Renderer =
        new GameBoyScreenRenderer(160, 144);



    public static int GetSelectedIndex() => _selectedIndex;


    public static bool ShouldShowGameScreen()
    {
        return UnityGbAdapter.GetState() == EmulatorState.Running
            && !BootScreenController.IsActive()
            && !_pendingStop;
    }


    public static bool HandleInput()
    {
        if (UnityGbAdapter.GetState() == EmulatorState.Running)
            return HandleRunningInput();
        else
            return HandleLibraryInput();
    }

    private static bool HandleRunningInput()
    {

        if (_pendingStop)
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Y) || UnityEngine.Input.GetKeyDown(KeyCode.Return))
            {
                _pendingStop = false;
                UnityGbAdapter.Stop();
                RomLibrary.Refresh();
                ClampSelectedIndex();
                return false;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.N) || UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                _pendingStop = false;
                return true;
            }

            return false;
        }


        if (UnityEngine.Input.GetKeyDown(KeyCode.Backspace))
        {
            _pendingStop = true;
            return false;
        }


        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            UnityGbAdapter.RestartCurrentRom();
            BootScreenController.StartBoot();
            return false;
        }

        return false;
    }

    private static bool HandleLibraryInput()
    {

        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            RomLibrary.Refresh();
            ClampSelectedIndex();
            return false;
        }

        var roms = RomLibrary.GetRoms();
        if (roms.Count == 0)
            return false;


        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
        {
            _selectedIndex++;
            ClampSelectedIndex();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
        {
            _selectedIndex--;
            ClampSelectedIndex();
        }


        if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
        {
            if (_selectedIndex >= 0 && _selectedIndex < roms.Count)
            {
                UnityGbAdapter.LoadRom(roms[_selectedIndex]);
                BootScreenController.StartBoot();
            }
        }

        return false;
    }


    public static Texture2D GetTexture()
    {
        if (BootScreenController.IsActive())
        {
            DrawBootScreen();
            return Renderer.GetTexture();
        }

        if (UnityGbAdapter.GetState() == EmulatorState.Running && _pendingStop)
        {
            DrawStopConfirm();
            return Renderer.GetTexture();
        }

        if (UnityGbAdapter.GetState() == EmulatorState.Error)
        {
            DrawErrorScreen();
            return Renderer.GetTexture();
        }

        DrawLibraryScreen();
        return Renderer.GetTexture();
    }


    private static void DrawLibraryScreen()
    {
        Renderer.Clear(GbDarkest);


        Renderer.FillRect(0, 0, 160, 10, GbDark);
        Renderer.DrawTextCentered(2, ">> CABINBOY <<", GbLightest);


        var roms = RomLibrary.GetRoms();
        ClampSelectedIndex();

        string countLabel = roms.Count == 1
            ? "1 ROM FOUND"
            : roms.Count + " ROMS FOUND";

        Renderer.DrawText(2, 12, countLabel, GbLight);


        Renderer.DrawHLine(0, 21, 160, GbDark);


        if (roms.Count == 0)
        {
            Renderer.DrawText(2, 30, "NO ROMS FOUND.", GbLight);
            Renderer.DrawText(2, 40, "ADD .GB/.GBC FILES TO:", GbLight);
            Renderer.DrawText(2, 50, "MODS/CABINBOY/ROMS/", GbLight);
        }
        else
        {
            for (int i = 0; i < VisibleRomSlots; i++)
            {
                int romIdx = _scrollOffset + i;
                if (romIdx >= roms.Count)
                    break;

                int     y        = 23 + i * 9;
                bool    selected = romIdx == _selectedIndex;
                Color32 fg       = selected ? GbLightest : GbLight;

                if (selected)
                    Renderer.FillRect(0, y - 1, 160, 9, GbDark);

                string prefix = selected ? ">" : " ";
                string label  = Truncate(roms[romIdx].CartridgeLabel, 22);

                Renderer.DrawText(2, y, prefix + " " + label, fg);
            }


            if (_scrollOffset > 0)
                Renderer.DrawText(152, 23, "^", GbLight);

            if (_scrollOffset + VisibleRomSlots < roms.Count)
                Renderer.DrawText(152, 86, "v", GbLight);
        }


        Renderer.DrawHLine(0, 96, 160, GbDark);


        Renderer.DrawText(2,  98, "ENTER:LOAD  R:SCAN", GbLight);
        Renderer.DrawText(2, 107, "F8:CLOSE", GbLight);
    }

    private static void DrawBootScreen()
    {
        Renderer.Clear(GbDarkest);

        Renderer.DrawTextCentered(28, "CABINBOY", GbLightest);
        Renderer.DrawTextCentered(40, "LZC4 MODDING", GbLight);

        Renderer.DrawHLine(20, 52, 120, GbDark);

        RomInfo rom = UnityGbAdapter.GetLoadedRom();

        if (rom != null)
        {
            string label = Truncate(rom.CartridgeLabel, 24);
            Renderer.DrawTextCentered(62, label, GbLightest);
        }

        int    dotCount = Mathf.FloorToInt(BootScreenController.GetTimer() * 3f) % 4;
        string dots     = new string('.', dotCount);
        Renderer.DrawTextCentered(82, "BOOTING" + dots, GbLight);
    }

    private static void DrawStopConfirm()
    {
        Renderer.Clear(GbDarkest);

        Renderer.DrawHLine(0, 0, 160, GbDark);
        Renderer.DrawTextCentered(2, ">> CABINBOY <<", GbLightest);
        Renderer.DrawHLine(0, 11, 160, GbDark);

        Renderer.DrawTextCentered(36, "STOP GAME?", GbLightest);

        Renderer.DrawHLine(20, 50, 120, GbDark);

        Renderer.DrawTextCentered(60, "Y / ENTER  =  YES", GbLightest);
        Renderer.DrawTextCentered(72, "N / ESC    =  NO", GbLight);
    }

    private static void DrawErrorScreen()
    {
        Renderer.Clear(GbDarkest);

        Renderer.FillRect(0, 0, 160, 10, GbDark);
        Renderer.DrawTextCentered(2, "!! ERROR !!", GbLightest);

        Renderer.DrawText(2, 20, "EMULATOR CRASHED.", GbLight);
        Renderer.DrawText(2, 30, "CHECK MELON LOADER LOG.", GbLight);

        Renderer.DrawHLine(0, 96, 160, GbDark);
        Renderer.DrawText(2,  98, "R:RETRY ROM", GbLight);
        Renderer.DrawText(2, 107, "BACKSPACE:RESET", GbLight);
    }



    private static void ClampSelectedIndex()
    {
        var roms = RomLibrary.GetRoms();

        if (roms.Count == 0)
        {
            _selectedIndex = 0;
            _scrollOffset  = 0;
            return;
        }

        if (_selectedIndex < 0)
            _selectedIndex = 0;

        if (_selectedIndex >= roms.Count)
            _selectedIndex = roms.Count - 1;

        UpdateScrollOffset();
    }

    private static void UpdateScrollOffset()
    {
        if (_selectedIndex < _scrollOffset)
            _scrollOffset = _selectedIndex;

        if (_selectedIndex >= _scrollOffset + VisibleRomSlots)
            _scrollOffset = _selectedIndex - VisibleRomSlots + 1;
    }

    private static string Truncate(string s, int max)
    {
        if (s == null) return "";
        if (s.Length <= max) return s;
        return s.Substring(0, max - 1) + "~";
    }
}
