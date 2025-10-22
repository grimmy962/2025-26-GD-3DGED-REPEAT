using Microsoft.Xna.Framework;

namespace GDEngine.Core.Timing
{
    /// <summary>
    /// Static time properties updated each frame that allow timescaling
    /// </summary>
    public static class Time
    {
        public static float DeltaTimeSecs { get; private set; }
        public static float UnscaledDeltaTimeSecs { get; private set; }
        public static float TimeScale { get; set; } = 1.0f;
        public static int FrameCount { get; private set; }
        public static double RealtimeSinceStartupSecs { get; private set; }

        public static void Update(GameTime gameTime)
        {
            UnscaledDeltaTimeSecs = (float)gameTime.ElapsedGameTime.TotalSeconds; //16ms
            DeltaTimeSecs = UnscaledDeltaTimeSecs * TimeScale;
            RealtimeSinceStartupSecs += UnscaledDeltaTimeSecs;
            FrameCount++;
        }
    }
}
