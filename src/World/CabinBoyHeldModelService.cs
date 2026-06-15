using System.IO;

using UnityEngine;

using CabinBoy.Config;
using CabinBoy.Core;
using CabinBoy.Emulator;
using CabinBoy.UI;

namespace CabinBoy.World;

public static class CabinBoyHeldModelService
{
    private static AssetBundle _bundle;
    private static GameObject  _prefab;
    private static GameObject  _instance;
    private static Renderer    _screenRenderer;
    private static int         _screenMaterialIndex = -1;
    private static Camera      _camera;


    private static float _swayTimer;


    private static bool  _animatingIn;
    private static bool  _animatingOut;
    private static float _animTimer;
    private const  float AnimDuration   = 0.40f;
    private const  float AnimStartDropY = -0.45f;

    private static float _distance;
    private static bool  _distanceLoaded;
    private static bool  _distanceDirty;
    private const  float MinDistance    = 0.25f;
    private const  float MaxDistance    = 1.20f;
    private const  float ZoomSpeed      = 0.50f;


    private static Texture2D _fitTexture;
    private static Color32[] _fitPixels;

    private const int FitTextureWidth  = 160;
    private const int FitTextureHeight = 144;
    private const int GameWidth        = 160;
    private const int GameHeight       = 144;



    public static void Update(bool shouldBeVisible)
    {
        if (shouldBeVisible)
        {
            if (_instance != null && _animatingOut)
            {
                _animatingOut = false;
                _animatingIn  = true;
                _animTimer    = 0f;
            }

            EnsureVisible();
            HandleZoomInput();
            UpdateHoverTransform();
            UpdateScreenTexture();
        }
        else
        {
            if (_instance == null)
                return;

            if (!_animatingOut)
            {
                _animatingOut = true;
                _animatingIn  = false;
                _animTimer    = 0f;
            }

            UpdateHoverTransform();
            UpdateScreenTexture();
        }
    }



    private static void EnsureDistanceLoaded()
    {
        if (_distanceLoaded)
            return;

        _distance = Mathf.Clamp(
            CabinBoyConfigManager.Config.HeldModelDistance,
            MinDistance, MaxDistance);

        _distanceLoaded = true;
    }

    private static void HandleZoomInput()
    {
        EnsureDistanceLoaded();

        float delta = 0f;

        if (UnityEngine.Input.GetKey(KeyCode.KeypadPlus)  ||
            UnityEngine.Input.GetKey(KeyCode.Plus)        ||
            UnityEngine.Input.GetKey(KeyCode.Equals))
            delta -= ZoomSpeed * Time.deltaTime;

        if (UnityEngine.Input.GetKey(KeyCode.KeypadMinus) ||
            UnityEngine.Input.GetKey(KeyCode.Minus))
            delta += ZoomSpeed * Time.deltaTime;

        if (delta == 0f)
            return;

        float updated = Mathf.Clamp(_distance + delta, MinDistance, MaxDistance);

        if (updated != _distance)
        {
            _distance      = updated;
            _distanceDirty = true;
        }
    }

    private static void EnsureVisible()
    {
        if (_instance != null)
            return;

        if (!LoadPrefab())
            return;

        _camera = FindBestCamera();

        if (_camera == null)
        {
            ModLogger.Warning("No active camera found.");
            return;
        }

        _instance      = Object.Instantiate(_prefab);
        _instance.name = "CabinBoy_HoverModel";
        _instance.SetActive(true);

        DisablePhysics(_instance);
        ForceVisible(_instance, _camera);
        FindScreenMaterialSlot(_instance);
        InitBlackScreen();

        _swayTimer   = 0f;
        _animTimer   = 0f;
        _animatingIn = true;

        ModLogger.Msg("CabinBoy hover model spawned.");
    }

    private static void Hide()
    {
        if (_instance == null)
            return;

        if (_distanceDirty)
        {
            CabinBoyConfigManager.Config.HeldModelDistance = _distance;
            CabinBoyConfigManager.Save();
            _distanceDirty = false;
        }

        Object.Destroy(_instance);

        _instance            = null;
        _screenRenderer      = null;
        _screenMaterialIndex = -1;
        _camera              = null;
        _animatingIn         = false;
        _animatingOut        = false;

        ModLogger.Msg("CabinBoy hover model hidden.");
    }



    private static void UpdateHoverTransform()
    {
        if (_instance == null)
            return;

        if (_camera == null || !_camera.gameObject.activeInHierarchy)
        {
            _camera = FindBestCamera();
            if (_camera == null) return;
        }

        _swayTimer += Time.deltaTime;

        float swayX = Mathf.Sin(_swayTimer * 1.15f) * 0.0025f;
        float swayY = Mathf.Sin(_swayTimer * 1.45f) * 0.0018f;

        Transform cam = _camera.transform;


        EnsureDistanceLoaded();

        Vector3 targetPos =
            cam.position
            + cam.forward * _distance
            + cam.right   * swayX
            + cam.up      * (-0.08f + swayY);

        Vector3 modelPos;

        if (_animatingIn)
        {
            _animTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_animTimer / AnimDuration);

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            Vector3 startPos = targetPos + cam.up * AnimStartDropY;
            modelPos = Vector3.LerpUnclamped(startPos, targetPos, eased);

            if (t >= 1f)
                _animatingIn = false;
        }
        else if (_animatingOut)
        {
            _animTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_animTimer / AnimDuration);

            float eased = t * t * t;

            Vector3 endPos = targetPos + cam.up * AnimStartDropY;
            modelPos = Vector3.LerpUnclamped(targetPos, endPos, eased);

            if (t >= 1f)
            {
                Hide();
                return;
            }
        }
        else
        {
            modelPos = targetPos;
        }

        _instance.transform.position   = modelPos;
        _instance.transform.rotation   = cam.rotation * Quaternion.Euler(-90f, 180f, 0f);
        _instance.transform.localScale = Vector3.one * 48f;
    }


    private static bool LoadPrefab()
    {
        if (_prefab != null)
            return true;

        if (_bundle == null)
        {
            string bundlePath = FindBundlePath();

            if (string.IsNullOrEmpty(bundlePath))
            {
                ModLogger.Warning("Held model bundle missing.");
                return false;
            }

            _bundle = AssetBundle.LoadFromFile(bundlePath);

            if (_bundle == null)
            {
                ModLogger.Error("Failed to load CabinBoy bundle.");
                return false;
            }
        }

        string[] assetNames = _bundle.GetAllAssetNames();

        for (int i = 0; i < assetNames.Length; i++)
        {
            GameObject asset = _bundle.LoadAsset<GameObject>(assetNames[i]);

            if (asset != null)
            {
                _prefab = asset;
                ModLogger.Msg("Loaded CabinBoy hover prefab: " + asset.name);
                break;
            }
        }

        return _prefab != null;
    }

    private static string FindBundlePath()
    {
        string folder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Mods", "CabinBoy", "AssetBundles");

        if (!Directory.Exists(folder))
            return null;

        string path = Path.Combine(folder, "cabinboy");

        return File.Exists(path) ? path : null;
    }



    private static Camera FindBestCamera()
    {
        Camera main = Camera.main;

        if (main != null && main.enabled && main.gameObject.activeInHierarchy)
            return main;

        Camera[] cameras   = Object.FindObjectsOfType<Camera>();
        Camera   best      = null;
        int      bestScore = int.MinValue;

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera cam = cameras[i];

            if (cam == null || !cam.enabled || !cam.gameObject.activeInHierarchy)
                continue;

            int    score = cam.pixelWidth * cam.pixelHeight;
            string name  = cam.name.ToLowerInvariant();

            if (name.Contains("main")) score += 100000000;
            if (name.Contains("fps"))  score += 100000000;

            if (score > bestScore)
            {
                bestScore = score;
                best      = cam;
            }
        }

        return best;
    }



    private static void DisablePhysics(GameObject root)
    {
        foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
            Object.Destroy(rb);

        foreach (var col in root.GetComponentsInChildren<Collider>(true))
            col.enabled = false;
    }

    private static void ForceVisible(GameObject root, Camera camera)
    {
        int layer = FindVisibleLayer(camera);
        SetLayerRecursive(root, layer);

        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            r.enabled           = true;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows    = false;
        }
    }

    private static int FindVisibleLayer(Camera camera)
    {
        if (camera == null) return 0;

        for (int layer = 0; layer < 32; layer++)
        {
            if ((camera.cullingMask & (1 << layer)) != 0)
                return layer;
        }

        return 0;
    }

    private static void SetLayerRecursive(GameObject root, int layer)
    {
        root.layer = layer;
        Transform t = root.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursive(t.GetChild(i).gameObject, layer);
    }



    private static void FindScreenMaterialSlot(GameObject root)
    {
        _screenRenderer      = null;
        _screenMaterialIndex = -1;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.materials;

            for (int j = 0; j < materials.Length; j++)
            {
                Material m = materials[j];
                if (m == null) continue;

                string mName = m.name.ToLowerInvariant();

                if (mName.Contains("gb_screen") ||
                    mName.Contains("screen")    ||
                    mName.Contains("display")   ||
                    mName.Contains("lcd"))
                {
                    _screenRenderer      = renderer;
                    _screenMaterialIndex = j;
                    ModLogger.Msg("Screen material: " + renderer.name + "[" + j + "] " + m.name);
                    return;
                }
            }
        }

        ModLogger.Warning("No screen material slot found.");
    }



    private static void InitBlackScreen()
    {
        if (_screenRenderer == null || _screenMaterialIndex < 0)
            return;

        Texture2D black = new Texture2D(1, 1, TextureFormat.RGB24, false);
        black.SetPixel(0, 0, Color.black);
        black.Apply();

        Material[] mats = _screenRenderer.materials;
        Material   mat  = mats[_screenMaterialIndex];
        if (mat == null) return;

        mat.mainTexture            = black;
        mat.mainTextureScale       = Vector2.one;
        mat.mainTextureOffset      = Vector2.zero;
        mats[_screenMaterialIndex] = mat;
        _screenRenderer.materials  = mats;
    }


    private static void UpdateScreenTexture()
    {
        if (_screenRenderer == null || _screenMaterialIndex < 0)
            return;

        Texture2D texToApply;

        if (ScreenUiController.ShouldShowGameScreen())
        {
            Texture2D gameTexture = UnityGbAdapter.GetScreenTexture();
            if (gameTexture == null) return;
            texToApply = BuildFitTexture(gameTexture);
        }
        else
        {
            texToApply = ScreenUiController.GetTexture();
            if (texToApply == null) return;
        }

        ApplyToMaterial(texToApply);
    }

    private static void ApplyToMaterial(Texture2D tex)
    {
        Material[] mats = _screenRenderer.materials;
        if (_screenMaterialIndex >= mats.Length) return;

        Material mat = mats[_screenMaterialIndex];
        if (mat == null) return;

        mat.mainTexture            = tex;
        mat.mainTextureScale       = Vector2.one;
        mat.mainTextureOffset      = Vector2.zero;
        mats[_screenMaterialIndex] = mat;
        _screenRenderer.materials  = mats;
    }



    private static Texture2D BuildFitTexture(Texture2D source)
    {
        if (_fitTexture == null)
        {
            _fitTexture = new Texture2D(
                FitTextureWidth, FitTextureHeight,
                TextureFormat.RGBA32, false);

            _fitTexture.filterMode = FilterMode.Point;
            _fitTexture.wrapMode   = TextureWrapMode.Clamp;
            _fitPixels = new Color32[FitTextureWidth * FitTextureHeight];

            ModLogger.Msg("Created game fit texture 160x144.");
        }

        Color32[] sourcePixels = source.GetPixels32();
        int       srcW         = source.width;
        int       srcH         = source.height;

        for (int y = 0; y < GameHeight; y++)
        {
            int srcY = y * srcH / GameHeight;

            for (int x = 0; x < GameWidth; x++)
            {
                int srcX = (GameWidth - 1 - x) * srcW / GameWidth;
                _fitPixels[y * FitTextureWidth + x] = sourcePixels[srcY * srcW + srcX];
            }
        }

        _fitTexture.SetPixels32(_fitPixels);
        _fitTexture.Apply(false);
        return _fitTexture;
    }
}
