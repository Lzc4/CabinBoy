namespace CabinBoy.Emulator;

public static class GameBoyTiming
{
    public const float TargetFramesPerSecond = 59.7f;

    private const int MaxFramesPerUpdate = 16;

    private static float _accumulator;

    public static int FramesToRun(float deltaTime, float speedMultiplier)
    {
        if (speedMultiplier < 1f)
            speedMultiplier = 1f;

        _accumulator += deltaTime * speedMultiplier;

        float frameTime = 1f / TargetFramesPerSecond;

        int frames = 0;

        while (_accumulator >= frameTime)
        {
            _accumulator -= frameTime;
            frames++;

            if (frames >= MaxFramesPerUpdate)
            {
                _accumulator = 0f;
                break;
            }
        }

        return frames;
    }

    public static void Reset()
    {
        _accumulator = 0f;
    }
}
