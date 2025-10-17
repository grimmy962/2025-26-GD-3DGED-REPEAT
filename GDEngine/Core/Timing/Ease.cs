namespace GDEngine.Core.Timing
{
    /// <summary>
    /// Minimal easing helpers for shaping a scalar in [0,1] → [0,1].
    /// Use these to remap normalized time before applying it (e.g., to a sine controller).
    /// Includes Linear, SineInOut, QuadInOut, CubicOut, SmoothStep, and ElasticOut.
    /// </summary>
    /// <see cref="https://easings.net/"/>
    /// <see cref="SineTranslateController"/>
    public static class Ease
    {
        #region Methods
        /// <summary>
        /// Linear mapping. Returns t unchanged.
        /// </summary>
        public static float Linear(float t)
        {
            return t;
        }

        /// <summary>
        /// Sine ease-in-out (slow at start and end, fast in the middle).
        /// Formula: -0.5 * (cos(pi * t) - 1).
        /// </summary>
        public static float SineInOut(float t)
        {
            return -0.5f * ((float)Math.Cos(Math.PI * t) - 1f);
        }

        /// <summary>
        /// Quadratic ease-in-out. Piecewise quadratic with smooth mid-point.
        /// </summary>
        public static float QuadInOut(float t)
        {
            if (t < 0.5f)
                return 2f * t * t;

            return -1f + (4f - 2f * t) * t;
        }

        /// <summary>
        /// Cubic ease-out (fast at start, slow at end).
        /// </summary>
        public static float CubicOut(float t)
        {
            t -= 1f;
            return t * t * t + 1f;
        }

        /// <summary>
        /// Smoothstep (Hermite) ease-in-out: t*t*(3 - 2*t).
        /// </summary>
        public static float SmoothStep(float t)
        {
            return t * t * (3f - 2f * t);
        }

        /// <summary>
        /// Elastic ease-out (overshoots and oscillates while settling at 1).
        /// Parameters tuned for a pleasantly bouncy default.
        /// </summary>
        public static float ElasticOut(float t)
        {
            if (t == 0f || t == 1f)
                return t;

            float p = 0.3f; // period
            return (float)(Math.Pow(2.0, -10.0 * t) * Math.Sin((t - p / 4f) * (2.0 * Math.PI) / p) + 1.0);
        }

        /// <summary>
        /// Exponential ease-in-out (very slow start/end, very fast middle).
        /// </summary>
        public static float ExpoInOut(float t)
        {
            // TODO - Homework
            throw new NotImplementedException();
        }

        /// <summary>
        /// Bounce ease-out (simulates decreasing bounces approaching 1).
        /// </summary>
        public static float BounceOut(float t)
        {
            // TODO - Homework
            throw new NotImplementedException();
        }
        #endregion
    }
}
