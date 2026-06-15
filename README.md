# 🎮 CabinBoy

> A working Game Boy emulator inside *The Long Dark* — sit by the fire and play your favorite cartridges on a handheld you carry in your pack.

CabinBoy adds a handheld console to *The Long Dark*. Equip it from your inventory and a Game Boy folds up into your hands, rendered in 3D, running real `.gb` / `.gbc` ROMs with picture **and** sound. Time keeps passing and your survivor still gets hungry — so don't get *too* lost in Kanto.

---

## ✨ Features

- 🕹️ **Full Game Boy emulation** (CPU, PPU, APU) via an embedded [UnityGB](https://github.com/Inkdrop/UnityGB)-based core
- 🔊 **Real sound** — audio is streamed straight to the Windows sound device (works even though TLD disables Unity's built-in audio)
- 🎒 **In-world handheld** — a 3D model flies into and out of your hands with a smooth animation
- 🔍 **Stepless zoom** — pull the screen closer or push it away
- ⏩ **Fast-forward** — hold a key to speed the emulation up
- 🔈 **Volume control** — adjust in-game, saved between sessions
- 💾 **Battery saves (SRAM)** — your in-game progress is written to disk per ROM
- 📚 **ROM library** — drop in as many ROMs as you like and pick from an on-device menu
- 🛖 **Survival-aware** — playing relieves Cabin Fever, but time, hunger, cold and fatigue keep ticking

---

## 📦 Requirements & Dependencies

| Dependency | Version | Notes |
|---|---|---|
| **The Long Dark** | 2.55 (Unity 6000.0.60f1, IL2CPP) | Windows only |
| **[MelonLoader](https://github.com/LavaGang/MelonLoader)** | 0.7.2 | Mod loader |
| **[ModComponent](https://github.com/DigitalzombieTLD/ModComponent)** | 7.0.0 | Provides the custom handheld item |

> ⚠️ **Windows only.** Audio output uses the Windows `winmm` (waveOut) API, so the mod will not produce sound on Linux/Proton without changes.

> 📜 **ROMs are not included.** You must supply your own legally-obtained `.gb` / `.gbc` files. Don't ask for or share copyrighted ROMs here.

---

## 🚀 Installation

1. **Install MelonLoader 0.7.2** into your The Long Dark folder (run the MelonLoader installer, point it at `tld.exe`).
2. **Install ModComponent** — drop `ModComponent.dll` into `TheLongDark/Mods/`.
3. **Install CabinBoy** — from the release archive, copy into your game folder so you end up with this layout:

```
TheLongDark/
└── Mods/
    ├── ModComponent.dll
    ├── CabinBoy.dll
    ├── cabinboy.modcomponent          ← the handheld item
    └── CabinBoy/
        ├── config.json                ← created on first launch
        ├── AssetBundles/
        │   └── cabinboy               ← 3D handheld model
        ├── Roms/                      ← put your .gb / .gbc files here
        └── Saves/                     ← SRAM battery saves (auto-created)
```

4. **Add ROMs** — drop `.gb` / `.gbc` files into `Mods/CabinBoy/Roms/`.
5. **Launch the game.** Acquire the CabinBoy item, then press **Play** on it in your inventory.

---

## 🔦 Where to find it

CabinBoy has **no fixed map location** — it spawns randomly via loot tables, so you can find it in these container types anywhere in the world:

| Container | Spawn weight |
|---|---|
| 🗄️ Desks | 2 (most likely) |
| 🗃️ Low drawers | 1 |
| 📦 Plastic boxes | 1 |
| 🎒 Backpacks | 1 |

> 💡 Loot in *The Long Dark* is randomized per save — keep searching desks and containers, especially in houses and offices.

---

## 🎛️ Controls

| Action | Key |
|---|---|
| Open handheld | **Play** on the item in your inventory |
| Close handheld | **F8** *(configurable)* |
| D-Pad | **W A S D** |
| A button | **J** |
| B button | **K** |
| Start | **Enter** |
| Select | **Right Shift** |
| Fast-forward (hold) | **Left Shift** |
| Zoom in / out (hold) | **+** / **−** |
| Volume up / down (hold) | **.** / **,** |
| Stop / change ROM | **Backspace** (confirm on screen) |
| Restart ROM | **R** |
| ROM library: navigate / load / rescan | **↑ ↓** / **Enter** / **R** |

---

## ⚙️ Configuration

A `config.json` is generated in `Mods/CabinBoy/` on first launch. Useful keys:

| Key | Default | Description |
|---|---|---|
| `RequiredHandheldGearName` | `GEAR_CabinBoy` | Item name the patches react to |
| `CloseKey` | `289` (F8) | Key to close the overlay ([Unity KeyCode](https://docs.unity3d.com/ScriptReference/KeyCode.html) value) |
| `ToggleKey` | `297` (F16) | Debug open/close toggle |
| `FastForwardMultiplier` | `4` | Emulation speed while holding Left Shift |
| `MasterVolume` | `0.6` | 0.0–1.0, adjusted live with `,` / `.` |
| `HeldModelDistance` | `0.55` | Handheld distance from camera, adjusted with `+` / `−` |
| `AutoLoadLastRom` | `true` | Reload the last played ROM on open |
| `EnableCabinFeverRelief` | `true` | Whether playing relieves Cabin Fever |
| `DebugOverlayEnabled` | `false` | Show the developer IMGUI panel |

---

## 🔨 Building from source

**Requirements:** .NET 6 SDK and a working MelonLoader install of The Long Dark (for the reference assemblies).

The project references DLLs from a local `Libs/` folder. Copy these out of your game's MelonLoader install into `Libs/`:

- `MelonLoader.dll`, `0Harmony.dll` *(from `MelonLoader/net6/`)*
- `Il2CppInterop.Runtime.dll`, `Il2Cppmscorlib.dll`, `Assembly-CSharp.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.IMGUIModule.dll`, `UnityEngine.InputLegacyModule.dll`, `UnityEngine.AssetBundleModule.dll`, `UnityEngine.PhysicsModule.dll` *(from `MelonLoader/Il2CppAssemblies/`)*

Then build:

```bash
dotnet build -c Release
```

The output `CabinBoy.dll` lands in `bin/Release/net6.0/`. Copy it into `TheLongDark/Mods/`.


---

## 🙏 Credits

- **CabinBoy** — Luca & Marcy (Patchworkers Union)
- **[UnityGB](https://github.com/Inkdrop/UnityGB)** — Game Boy emulation core
- **[MelonLoader](https://github.com/LavaGang/MelonLoader)** & **[ModComponent](https://github.com/DigitalzombieTLD/ModComponent)** — modding framework

---

## ⚖️ Legal

This project does not contain or distribute any Nintendo ROMs, BIOS files, or copyrighted assets. "Game Boy" is a trademark of Nintendo. Use only ROMs you are legally entitled to use. This is a non-commercial fan project and is not affiliated with Nintendo or Hinterland Studio.
