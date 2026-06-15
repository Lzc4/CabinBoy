namespace CabinBoy.Handheld;

public static class HandheldUsageStats
{
    private static float _currentSessionSeconds;
    private static int _reliefTicks;

    public static float CurrentSessionSeconds
    {
        get { return _currentSessionSeconds; }
    }

    public static int ReliefTicks
    {
        get { return _reliefTicks; }
    }

    public static void AddSessionTime(float deltaTime)
    {
        _currentSessionSeconds += deltaTime;
    }

    public static void AddReliefTick()
    {
        _reliefTicks++;
    }

    public static void ResetSession()
    {
        _currentSessionSeconds = 0f;
        _reliefTicks = 0;
    }
}
