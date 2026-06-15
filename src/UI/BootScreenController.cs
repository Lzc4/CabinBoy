using UnityEngine;

namespace CabinBoy.UI;


public static class BootScreenController
{
    private const float Duration = 2.8f;

    private static bool  _active;
    private static float _timer;


    public static void StartBoot()
    {
        _active = true;
        _timer  = 0f;
    }

    public static void Update(float deltaTime)
    {
        if (!_active)
            return;

        _timer += deltaTime;

        if (_timer >= Duration)
            _active = false;
    }

    public static bool  IsActive()  => _active;
    public static float GetTimer()  => _timer;
    public static float Progress    => Mathf.Clamp01(_timer / Duration);


    public static void Draw(Rect rect)
    {
        if (!_active)
            return;

        GUI.color = new Color(0.06f, 0.09f, 0.06f, 1f);
        GUI.Box(rect, "");

        GUI.color = new Color(0.65f, 0.82f, 0.60f, 1f);

        GUILayout.BeginArea(rect);
        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        GUILayout.Label("CABINBOY\u2122");
        GUILayout.Space(14f);
        GUILayout.Label("LZC4 MODDING");
        GUILayout.Space(24f);
        GUILayout.Label("Portable Entertainment System");
        GUILayout.Space(18f);

        int    dotCount = Mathf.FloorToInt(_timer * 3f) % 4;
        string dots     = new string('.', dotCount);
        GUILayout.Label("BOOTING" + dots);

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.EndArea();

        GUI.color = Color.white;
    }
}
