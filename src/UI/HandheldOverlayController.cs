using UnityEngine;

using CabinBoy.Config;
using CabinBoy.Emulator;
using CabinBoy.Handheld;

namespace CabinBoy.UI;


public static class HandheldOverlayController
{
    private static bool    _isVisible;
    private static bool    _pendingOpen;
    private static Vector2 _scrollPosition = Vector2.zero;

    public static bool IsVisible() => _isVisible;

    public static void Toggle()
    {
        if (_isVisible) Close();
        else            Open();
    }

    public static void RequestOpen()
    {
        _pendingOpen = true;
    }

    public static void Open()
    {
        if (!HandheldInventoryService.CanUseHandheld())
            return;

        _isVisible = true;

        RomLibrary.Refresh();

        bool autoLoaded = UnityGbAdapter.TryAutoLoadLastRom();

        if (autoLoaded)
            BootScreenController.StartBoot();
    }

    public static void Close()
    {
        _isVisible = false;

        if (_volumeDirty)
        {
            CabinBoyConfigManager.Save();
            _volumeDirty = false;
        }
    }

    private static bool _volumeDirty;

    private const float VolumeSpeed = 0.6f;

    private static void HandleVolumeInput()
    {
        float delta = 0f;

        if (UnityEngine.Input.GetKey(KeyCode.Period))
            delta += VolumeSpeed * Time.deltaTime;

        if (UnityEngine.Input.GetKey(KeyCode.Comma))
            delta -= VolumeSpeed * Time.deltaTime;

        if (delta == 0f)
            return;

        float v = CabinBoyConfigManager.Config.MasterVolume + delta;

        if (v < 0f) v = 0f;
        else if (v > 1f) v = 1f;

        if (v != CabinBoyConfigManager.Config.MasterVolume)
        {
            CabinBoyConfigManager.Config.MasterVolume = v;
            _volumeDirty = true;
        }
    }



    public static void HandleInput()
    {
        if (_pendingOpen)
        {
            _pendingOpen = false;
            Open();
        }

        if (!_isVisible)
            return;

        ScreenUiController.HandleInput();

        HandleVolumeInput();

        if (UnityEngine.Input.GetKeyDown((KeyCode)CabinBoyConfigManager.Config.CloseKey))
            Close();
    }



    public static void Draw()
    {
        if (!_isVisible)
            return;

        if (!CabinBoyConfigManager.Config.DebugOverlayEnabled)
            return;


        Rect panelRect = GetCenteredPanelRect();

        Color oldColor        = GUI.color;
        Color oldContentColor = GUI.contentColor;

        GUI.color = new Color(0.02f, 0.02f, 0.02f, 0.98f);
        GUI.Box(panelRect, "");

        GUI.contentColor = Color.white;

        Rect contentRect = new Rect(
            panelRect.x + 24f,
            panelRect.y + 20f,
            panelRect.width  - 48f,
            panelRect.height - 40f);

        GUILayout.BeginArea(contentRect);

        DrawTitle();

        GUILayout.Space(12f);

        if (UnityGbAdapter.GetState() == EmulatorState.Running)
            DrawRunningMode();
        else
            DrawLibraryMode();

        GUILayout.EndArea();

        Rect exitRect = new Rect(
            panelRect.x + panelRect.width - 92f,
            panelRect.y + 12f,
            72f,
            28f);

        GUI.color        = new Color(0.35f, 0.10f, 0.10f, 1f);
        GUI.contentColor = Color.white;

        if (GUI.Button(exitRect, "EXIT"))
            Close();

        GUI.color = new Color(0.02f, 0.02f, 0.02f, 0.98f);


        if (BootScreenController.IsActive())
        {
            Rect bootRect = new Rect(
                panelRect.x + 50f,
                panelRect.y + 70f,
                panelRect.width  - 100f,
                panelRect.height - 140f);

            BootScreenController.Draw(bootRect);
        }

        GUI.color        = oldColor;
        GUI.contentColor = oldContentColor;
    }


    private static Rect GetCenteredPanelRect()
    {
        float w = CabinBoyConfigManager.Config.OverlayWidth;
        float h = CabinBoyConfigManager.Config.OverlayHeight;

        float x = Mathf.Max((Screen.width  - w) / 2f, 10f);
        float y = Mathf.Max((Screen.height - h) / 2f, 10f);

        return new Rect(x, y, w, h);
    }

    private static void DrawTitle()
    {
        GUI.contentColor = Color.white;
        GUILayout.Label("CABINBOY");

        GUI.contentColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        RomInfo loaded = UnityGbAdapter.GetLoadedRom();
        string  rom    = loaded != null ? loaded.FileName : "none";

        GUILayout.Label(
            "State: " + UnityGbAdapter.GetState() +
            "  |  ROM: " + rom);

        GUILayout.Label(
            "Auto-load: " + CabinBoyConfigManager.Config.AutoLoadLastRom +
            "  |  DevBypass: " + HandheldInventoryService.IsDevModeEnabled());
    }

    private static void DrawRunningMode()
    {
        GUILayout.Label("WASD=D-Pad  J=A  K=B  Enter=Start  RShift=Select");
        GUILayout.Label("Backspace=Stop (confirm on-screen)  R=Restart  F8/Esc=Close");

        GUILayout.Space(14f);

        DrawScreen();

        GUILayout.Space(12f);

        DrawUsageStats();

        GUILayout.Space(10f);

        RomInfo loaded = UnityGbAdapter.GetLoadedRom();

        if (loaded != null)
        {
            GUI.contentColor = new Color(0.80f, 0.92f, 0.70f, 1f);
            GUILayout.Label("CARTRIDGE: " + loaded.CartridgeLabel);
            GUI.contentColor = Color.white;
        }

        GUILayout.Label(
            "Input locked: " + PlayerInputLockService.IsLocked());
    }

    private static void DrawLibraryMode()
    {
        var roms = RomLibrary.GetRoms();

        GUILayout.Label("ROM Library  (" + roms.Count + " found)");
        GUILayout.Label("Up/Down=Select  Enter=Load  R=Scan  Esc=Close");

        GUILayout.Space(16f);

        if (roms.Count == 0)
        {
            GUILayout.Label("No ROMs found.");
            GUILayout.Label("Put .gb/.gbc files into  Mods/CabinBoy/Roms/");
            return;
        }

        int selIdx = ScreenUiController.GetSelectedIndex();

        _scrollPosition = GUILayout.BeginScrollView(
            _scrollPosition, GUILayout.Height(320f));

        for (int i = 0; i < roms.Count; i++)
        {
            GUI.contentColor = (i == selIdx)
                ? Color.white
                : new Color(0.70f, 0.70f, 0.70f, 1f);

            GUILayout.Label((i == selIdx ? "> " : "  ") + roms[i].CartridgeLabel);
        }

        GUILayout.EndScrollView();

        GUI.contentColor = Color.white;
        GUILayout.Space(8f);

        if (selIdx >= 0 && selIdx < roms.Count)
            GUILayout.Label("Selected: " + roms[selIdx].DisplayTitle);

        GUILayout.Label("Put .gb / .gbc files into  Mods/CabinBoy/Roms/");
    }

    private static void DrawScreen()
    {
        Texture2D texture = UnityGbAdapter.GetScreenTexture();

        Rect screenRect = GUILayoutUtility.GetRect(
            560f, 380f,
            GUILayout.ExpandWidth(false),
            GUILayout.ExpandHeight(false));

        GUI.color = new Color(0.10f, 0.10f, 0.10f, 1f);
        GUI.Box(screenRect, "");

        Rect inner = new Rect(
            screenRect.x + 20f, screenRect.y + 20f,
            screenRect.width - 40f, screenRect.height - 40f);

        GUI.color = Color.white;

        if (texture != null)
            GUI.DrawTexture(inner, texture, ScaleMode.ScaleToFit, false);
        else
        {
            GUI.contentColor = Color.white;
            GUI.Label(inner, "Waiting for video output...");
        }
    }

    private static void DrawUsageStats()
    {
        int total   = (int)HandheldUsageStats.CurrentSessionSeconds;
        int minutes = total / 60;
        int secs    = total % 60;

        GUI.contentColor = Color.white;
        GUILayout.Label("Session: " + minutes + "m " + secs + "s");
        GUILayout.Label("Relief ticks: " + HandheldUsageStats.ReliefTicks);
    }
}
