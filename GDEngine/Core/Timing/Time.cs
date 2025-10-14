using Microsoft.Xna.Framework;

namespace GDEngine.Core.Timing
{
    /// <summary>
    /// Static time properties updated each frame that allow timescaling
    /// </summary>
    public static class Time
    {
        public static float DeltaTime { get; private set; }
        public static float UnscaledDeltaTime { get; private set; }
        public static float TimeScale { get; set; } = 1.0f;
        public static int FrameCount { get; private set; }
        public static double RealtimeSinceStartup { get; private set; }

        public static void Update(GameTime gameTime)
        {
            UnscaledDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds; //16ms
            DeltaTime = UnscaledDeltaTime * TimeScale;
            RealtimeSinceStartup += UnscaledDeltaTime;
            FrameCount++;
        }
    }
}
