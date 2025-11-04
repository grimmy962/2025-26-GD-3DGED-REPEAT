#nullable enable
using Microsoft.Xna.Framework;

namespace GDEngine.Core
{
    /// <summary>
    /// Screen resolution presets plus aspect/letterbox helpers using <see cref="Integer2"/>.
    /// </summary>
    /// <see cref="Integer2"/>
    public static class ScreenResolution
    {
        #region Static Fields
        // 16:9
        public static readonly Integer2 R1280x720 = new Integer2(1280, 720);
        public static readonly Integer2 R1920x1080 = new Integer2(1920, 1080);
        public static readonly Integer2 R2560x1440 = new Integer2(2560, 1440);
        public static readonly Integer2 R3840x2160 = new Integer2(3840, 2160);

        // 16:10
        public static readonly Integer2 R1280x800 = new Integer2(1280, 800);
        public static readonly Integer2 R1440x900 = new Integer2(1440, 900);
        public static readonly Integer2 R1920x1200 = new Integer2(1920, 1200);
        public static readonly Integer2 R2560x1600 = new Integer2(2560, 1600);

        // 4:3
        public static readonly Integer2 R640x480 = new Integer2(640, 480);
        public static readonly Integer2 R1024x768 = new Integer2(1024, 768);
        public static readonly Integer2 R1280x960 = new Integer2(1280, 960);
        public static readonly Integer2 R1600x1200 = new Integer2(1600, 1200);
        public static readonly Integer2 R2048x1536 = new Integer2(2048, 1536);

        #endregion

        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Methods
        /// <summary>
        /// Computes greatest common divisor (Euclid) for aspect reduction.
        /// </summary>
        /// <see cref="Integer2"/>
        public static int Gcd(int a, int b)
        {
            if (a < 0) a = -a;
            if (b < 0) b = -b;
            if (a == 0)
                return b;
            if (b == 0)
                return a;

            while (b != 0)
            {
                int t = b;
                b = a % b;
                a = t;
            }

            if (a == 0)
                return 1;

            return a;
        }

        /// <summary>
        /// Returns width/height as a float aspect ratio (0 if height is 0).
        /// </summary>
        /// <see cref="Integer2"/>
        public static float Aspect(this Integer2 size)
        {
            if (size.Y <= 0)
                return 0f;

            return (float)size.X / size.Y;
        }

        /// <summary>
        /// Returns width/height as a float aspect ratio (0 if height is 0).
        /// </summary>
        public static float Aspect(int width, int height)
        {
            if (height <= 0)
                return 0f;

            return (float)width / height;
        }

        /// <summary>
        /// Reduces a size to its simplest integer aspect pair (e.g., 1920×1080 -> 16×9).
        /// </summary>
        /// <see cref="Integer2"/>
        public static Integer2 ReduceAspect(Integer2 size)
        {
            int g = Gcd(size.X, size.Y);
            if (g == 0)
                return new Integer2(0, 0);

            return new Integer2(size.X / g, size.Y / g);
        }

        /// <summary>
        /// Sets the preferred backbuffer size from width/height integers and calls ApplyChanges().
        /// </summary>
        public static void SetResolution(GraphicsDeviceManager graphicsDeviceManager, 
            int width, int height)
        {
            graphicsDeviceManager.PreferredBackBufferWidth = Math.Max(1, width);
            graphicsDeviceManager.PreferredBackBufferHeight = Math.Max(1, height);

            graphicsDeviceManager.ApplyChanges();
        }

        /// <summary>
        /// Sets the preferred backbuffer size from an Integer2 and calls ApplyChanges().
        /// </summary>
        /// <see cref="Integer2"/>
        public static void SetResolution(GraphicsDeviceManager graphicsDeviceManager, Integer2 resolution)
        {
            SetResolution(graphicsDeviceManager, resolution.X, resolution.Y);
        }

        /// <summary>
        /// Gets the current backbuffer size as an <see cref="Integer2"/>.
        /// Uses live PresentationParameters when available; otherwise falls back to preferred size.
        /// </summary>
        /// <see cref="Integer2"/>
        public static Integer2 GetResolution(GraphicsDeviceManager graphicsDeviceManager)
        {
            var graphicsDevice = graphicsDeviceManager.GraphicsDevice;

            // If we actually have a graphics device then read its w,h
            if (graphicsDevice != null)
            {
                var pp = graphicsDevice.PresentationParameters;
                return new Integer2(Math.Max(1, pp.BackBufferWidth),
                    Math.Max(1, pp.BackBufferHeight));
            }

            // Otherwise fall back on preferred
            return new Integer2(Math.Max(1, graphicsDeviceManager.PreferredBackBufferWidth),
                Math.Max(1, graphicsDeviceManager.PreferredBackBufferHeight));
        }
        #endregion

        #region Lifecycle Methods
        #endregion

        #region Housekeeping Methods
        #endregion
    }
}
