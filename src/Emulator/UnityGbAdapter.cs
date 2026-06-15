using System.IO;

using UnityEngine;
using UnityGB;

using CabinBoy.Config;
using CabinBoy.Core;
using CabinBoy.Handheld;

namespace CabinBoy.Emulator;

public static class UnityGbAdapter
{
    private static global::UnityGB.Emulator _emulator;

    private static EmulatorState _state = EmulatorState.Stopped;

    private static RomInfo _loadedRom;

    private static CabinBoyVideoOutput _videoOutput;
    private static CabinBoyAudioOutput _audioOutput;
    private static CabinBoySaveMemory _saveMemory;



    public static EmulatorState GetState() => _state;

    public static RomInfo GetLoadedRom() => _loadedRom;

    public static Texture2D GetScreenTexture()
        => _videoOutput?.GetTexture();


    public static void LoadRom(RomInfo rom)
    {
        if (rom == null)
        {
            ModLogger.Error("Tried to load null ROM.");
            return;
        }

        if (!File.Exists(rom.FullPath))
        {
            ModLogger.Error("ROM file missing: " + rom.FullPath);
            _state = EmulatorState.Error;
            return;
        }

        try
        {
            _state = EmulatorState.Loading;

            ModLogger.Msg("Loading ROM: " + rom.FileName);

            CabinFeverReliefService.Reset();
            GameBoyTiming.Reset();

            _audioOutput?.Dispose();

            _videoOutput = new CabinBoyVideoOutput();
            _audioOutput = new CabinBoyAudioOutput();
            _saveMemory  = new CabinBoySaveMemory();

            _emulator = new global::UnityGB.Emulator(
                _videoOutput,
                _audioOutput,
                _saveMemory);

            byte[] romBytes = File.ReadAllBytes(rom.FullPath);

            _emulator.LoadRom(romBytes);

            _loadedRom = rom;

            CabinBoyConfigManager.Config.LastLoadedRomPath = rom.FullPath;
            CabinBoyConfigManager.Save();

            _state = EmulatorState.Running;

            ModLogger.Msg("ROM loaded: " + rom.CartridgeLabel);
        }
        catch (System.Exception ex)
        {
            _state = EmulatorState.Error;
            ModLogger.Error("Failed to load ROM: " + ex);
        }
    }


    public static void RestartCurrentRom()
    {
        RomInfo current = _loadedRom;

        if (current == null)
        {
            ModLogger.Warning("RestartCurrentRom called with no ROM loaded.");
            return;
        }

        ModLogger.Msg("Restarting ROM: " + current.CartridgeLabel);

        Stop();
        LoadRom(current);
    }


    public static bool TryAutoLoadLastRom()
    {
        if (!CabinBoyConfigManager.Config.AutoLoadLastRom)
            return false;

        if (_state == EmulatorState.Running)
            return false;

        RomInfo lastRom =
            RomLibrary.FindByFullPath(
                CabinBoyConfigManager.Config.LastLoadedRomPath);

        if (lastRom == null)
            return false;

        LoadRom(lastRom);

        return true;
    }



    public static void Update()
    {
        if (_state != EmulatorState.Running)
            return;

        if (_emulator == null)
            return;


        HandheldUsageStats.AddSessionTime(Time.deltaTime);

        float speed = GetSpeedMultiplier();

        int frames = GameBoyTiming.FramesToRun(Time.deltaTime, speed);

        try
        {
            if (frames > 0)
            {
                HandleInput();

                for (int i = 0; i < frames; i++)
                    _emulator.RunNextStep();
            }

            _audioOutput?.Pump();
        }
        catch (System.Exception ex)
        {
            _state = EmulatorState.Error;
            ModLogger.Error("Emulator update failed: " + ex);
        }
    }



    public static void Stop()
    {
        ModLogger.Msg("Stopping emulator.");

        _audioOutput?.Dispose();

        _emulator    = null;
        _loadedRom   = null;
        _videoOutput = null;
        _audioOutput = null;
        _saveMemory  = null;

        CabinFeverReliefService.Reset();
        GameBoyTiming.Reset();

        _state = EmulatorState.Stopped;
    }


    private static float GetSpeedMultiplier()
    {
        bool fast = UnityEngine.Input.GetKey(KeyCode.LeftShift);

        return fast
            ? Mathf.Max(1f, CabinBoyConfigManager.Config.FastForwardMultiplier)
            : 1f;
    }

    private static void HandleInput()
    {
        SetButton(EmulatorBase.Button.Up,     KeyCode.W);
        SetButton(EmulatorBase.Button.Down,   KeyCode.S);
        SetButton(EmulatorBase.Button.Left,   KeyCode.A);
        SetButton(EmulatorBase.Button.Right,  KeyCode.D);
        SetButton(EmulatorBase.Button.A,      KeyCode.J);
        SetButton(EmulatorBase.Button.B,      KeyCode.K);
        SetButton(EmulatorBase.Button.Start,  KeyCode.Return);
        SetButton(EmulatorBase.Button.Select, KeyCode.RightShift);
    }

    private static void SetButton(EmulatorBase.Button button, KeyCode key)
    {
        if (_emulator == null)
            return;

        _emulator.SetInput(button, UnityEngine.Input.GetKey(key));
    }
}
