namespace GDEngine.Core.Timing
{
    /// <summary>
    /// Easing functions matching the set and formulas from easings.net.
    /// Use PascalCase names (e.g., EaseInSine) and x ∈ [0,1] → y (typically in [0,1]).
    /// </summary>
    /// <see cref="https://easings.net/"/>
    public static class Ease
    {
        #region Methods
        /// <summary>Linear: y = x.</summary>
        public static float Linear(float x)
        {
            return x;
        }

        // --- Sine ---

        /// <summary>EaseInSine: slow start, fast end.</summary>
        public static float EaseInSine(float x)
        {
            return 1f - (float)Math.Cos((x * Math.PI) / 2f);
        }

        /// <summary>EaseOutSine: fast start, slow end.</summary>
        public static float EaseOutSine(float x)
        {
            return (float)Math.Sin((x * Math.PI) / 2f);
        }

        /// <summary>EaseInOutSine: slow at start and end.</summary>
        public static float EaseInOutSine(float x)
        {
            return -0.5f * ((float)Math.Cos(Math.PI * x) - 1f);
        }

        // --- Quadratic ---

        /// <summary>EaseInQuad.</summary>
        public static float EaseInQuad(float x)
        {
            return x * x;
        }

        /// <summary>EaseOutQuad.</summary>
        public static float EaseOutQuad(float x)
        {
            return 1f - (1f - x) * (1f - x);
        }

        /// <summary>EaseInOutQuad.</summary>
        public static float EaseInOutQuad(float x)
        {
            if (x < 0.5f)
                return 2f * x * x;

            return 1f - (float)Math.Pow(-2f * x + 2f, 2f) / 2f;
        }

        // --- Cubic ---

        /// <summary>EaseInCubic.</summary>
        public static float EaseInCubic(float x)
        {
            return x * x * x;
        }

        /// <summary>EaseOutCubic.</summary>
        public static float EaseOutCubic(float x)
        {
            return 1f - (float)Math.Pow(1f - x, 3f);
        }

        /// <summary>EaseInOutCubic.</summary>
        public static float EaseInOutCubic(float x)
        {
            if (x < 0.5f)
                return 4f * x * x * x;

            return 1f - (float)Math.Pow(-2f * x + 2f, 3f) / 2f;
        }

        // --- Quartic ---

        /// <summary>EaseInQuart.</summary>
        public static float EaseInQuart(float x)
        {
            return x * x * x * x;
        }

        /// <summary>EaseOutQuart.</summary>
        public static float EaseOutQuart(float x)
        {
            return 1f - (float)Math.Pow(1f - x, 4f);
        }

        /// <summary>EaseInOutQuart.</summary>
        public static float EaseInOutQuart(float x)
        {
            if (x < 0.5f)
                return 8f * x * x * x * x;

            return 1f - (float)Math.Pow(-2f * x + 2f, 4f) / 2f;
        }

        // --- Quintic ---

        /// <summary>EaseInQuint.</summary>
        public static float EaseInQuint(float x)
        {
            return x * x * x * x * x;
        }

        /// <summary>EaseOutQuint.</summary>
        public static float EaseOutQuint(float x)
        {
            return 1f - (float)Math.Pow(1f - x, 5f);
        }

        /// <summary>EaseInOutQuint.</summary>
        public static float EaseInOutQuint(float x)
        {
            if (x < 0.5f)
                return 16f * x * x * x * x * x;

            return 1f - (float)Math.Pow(-2f * x + 2f, 5f) / 2f;
        }

        // --- Exponential ---

        /// <summary>EaseInExpo. Returns 0 at x=0.</summary>
        public static float EaseInExpo(float x)
        {
            if (x <= 0f)
                return 0f;

            return (float)Math.Pow(2f, 10f * x - 10f);
        }

        /// <summary>EaseOutExpo. Returns 1 at x=1.</summary>
        public static float EaseOutExpo(float x)
        {
            if (x >= 1f)
                return 1f;

            return 1f - (float)Math.Pow(2f, -10f * x);
        }

        /// <summary>EaseInOutExpo. Returns 0 at x=0 and 1 at x=1.</summary>
        public static float EaseInOutExpo(float x)
        {
            if (x <= 0f)
                return 0f;

            if (x >= 1f)
                return 1f;

            if (x < 0.5f)
                return (float)Math.Pow(2f, 20f * x - 10f) / 2f;

            return (2f - (float)Math.Pow(2f, -20f * x + 10f)) / 2f;
        }

        // --- Circular ---

        /// <summary>EaseInCirc.</summary>
        public static float EaseInCirc(float x)
        {
            return 1f - (float)Math.Sqrt(1f - x * x);
        }

        /// <summary>EaseOutCirc.</summary>
        public static float EaseOutCirc(float x)
        {
            return (float)Math.Sqrt(1f - Math.Pow(x - 1f, 2f));
        }

        /// <summary>EaseInOutCirc.</summary>
        public static float EaseInOutCirc(float x)
        {
            if (x < 0.5f)
                return (1f - (float)Math.Sqrt(1f - Math.Pow(2f * x, 2f))) / 2f;

            return ((float)Math.Sqrt(1f - Math.Pow(-2f * x + 2f, 2f)) + 1f) / 2f;
        }

        // --- Back ---

        /// <summary>EaseInBack (overshoots behind the start).</summary>
        public static float EaseInBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * x * x * x - c1 * x * x;
        }

        /// <summary>EaseOutBack (overshoots past the end).</summary>
        public static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float t = x - 1f;
            return 1f + c3 * t * t * t + c1 * t * t;
        }

        /// <summary>EaseInOutBack.</summary>
        public static float EaseInOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            if (x < 0.5f)
            {
                float t = 2f * x;
                return (t * t * ((c2 + 1f) * t - c2)) / 2f;
            }

            {
                float t = 2f * x - 2f;
                return (t * t * ((c2 + 1f) * t + c2) + 2f) / 2f;
            }
        }

        // --- Elastic ---

        /// <summary>EaseInElastic.</summary>
        public static float EaseInElastic(float x)
        {
            if (x <= 0f)
                return 0f;

            if (x >= 1f)
                return 1f;

            const float c4 = (2f * (float)Math.PI) / 3f;
            return -(float)Math.Pow(2f, 10f * x - 10f) * (float)Math.Sin((x * 10f - 10.75f) * c4);
        }

        /// <summary>EaseOutElastic.</summary>
        public static float EaseOutElastic(float x)
        {
            if (x <= 0f)
                return 0f;

            if (x >= 1f)
                return 1f;

            const float c4 = (2f * (float)Math.PI) / 3f;
            return (float)Math.Pow(2f, -10f * x) * (float)Math.Sin((x * 10f - 0.75f) * c4) + 1f;
        }

        /// <summary>EaseInOutElastic.</summary>
        public static float EaseInOutElastic(float x)
        {
            if (x <= 0f)
                return 0f;

            if (x >= 1f)
                return 1f;

            const float c5 = (2f * (float)Math.PI) / 4.5f;

            if (x < 0.5f)
                return -0.5f * (float)Math.Pow(2f, 20f * x - 10f) * (float)Math.Sin((20f * x - 11.125f) * c5);

            return 0.5f * (float)Math.Pow(2f, -20f * x + 10f) * (float)Math.Sin((20f * x - 11.125f) * c5) + 1f;
        }

        // --- Bounce ---

        /// <summary>EaseOutBounce.</summary>
        public static float EaseOutBounce(float x)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (x < 1f / d1)
                return n1 * x * x;

            if (x < 2f / d1)
            {
                x -= 1.5f / d1;
                return n1 * x * x + 0.75f;
            }

            if (x < 2.5f / d1)
            {
                x -= 2.25f / d1;
                return n1 * x * x + 0.9375f;
            }

            x -= 2.625f / d1;
            return n1 * x * x + 0.984375f;
        }

        /// <summary>EaseInBounce.</summary>
        public static float EaseInBounce(float x)
        {
            return 1f - EaseOutBounce(1f - x);
        }

        /// <summary>EaseInOutBounce.</summary>
        public static float EaseInOutBounce(float x)
        {
            if (x < 0.5f)
                return (1f - EaseOutBounce(1f - 2f * x)) / 2f;

            return (1f + EaseOutBounce(2f * x - 1f)) / 2f;
        }
        #endregion
    }
}
