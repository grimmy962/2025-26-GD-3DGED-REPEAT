using Microsoft.Xna.Framework;

namespace GDLibrary.Core.Timing
{
    /// <summary>
    /// 2D (x,y) animation curve composed of two scalar curves sharing the same time domain.
    /// Looping/clamping/ping-pong behavior is taken from the supplied <see cref="CurveLoopType"/> and respected by <see cref="Evaluate(double, int)"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// // Animate a 2D point along a simple path over 3 seconds; it loops based on the chosen loop type.
    /// var curve2 = new AnimationCurve2D(CurveLoopType.Cycle);
    /// curve2.AddKey(new Vector2(0, 0),     0);
    /// curve2.AddKey(new Vector2(4, 2),  1500);
    /// curve2.AddKey(new Vector2(0,-2),  3000);
    ///
    /// _elapsedMs += (int)(Time.DeltaTime * 1000f);
    /// Vector2 p = curve2.Evaluate(_elapsedMs); // honors CurveLoopType
    ///
    /// </code>
    /// </example>
    /// <see cref="AnimationCurve"/>
    /// <see cref="AnimationCurve3D"/>
    public class AnimationCurve2D
    {
        #region Fields

        private readonly AnimationCurve _x;
        private readonly AnimationCurve _y;

        #endregion

        #region Properties

        public CurveLoopType LoopType => _x.LoopType;

        public int KeyCount => Math.Max(_x.KeyCount, _y.KeyCount);

        public bool IsEmpty => _x.IsEmpty && _y.IsEmpty;

        public int StartMs
        {
            get
            {
                int sx = _x.IsEmpty ? int.MaxValue : _x.StartMs;
                int sy = _y.IsEmpty ? int.MaxValue : _y.StartMs;
                int s = Math.Min(sx, sy); 
                return s == int.MaxValue ? 0 : s;
            }
        }

        public int EndMs => Math.Max(_x.EndMs, _y.EndMs);

        public int DurationMs => IsEmpty ? 0 : EndMs - StartMs;

        #endregion

        #region Constructors

        public AnimationCurve2D(CurveLoopType loopType)
        {
            _x = new AnimationCurve(loopType);
            _y = new AnimationCurve(loopType);
        }

        #endregion

        #region Methods

        public void AddKey(Vector2 value, int timeInMs)
        {
            _x.AddKey(value.X, timeInMs);
            _y.AddKey(value.Y, timeInMs);
        }

        public bool SetValue(int index, Vector2 newValue)
        {
            bool a = _x.SetValue(index, newValue.X);
            bool b = _y.SetValue(index, newValue.Y);
            return a || b;
        }

        public void Clear()
        {
            _x.Clear();
            _y.Clear();
        }

        public Vector2 Evaluate(double timeInMs, int decimalPrecision = -1)
        {
            return new Vector2(
                _x.Evaluate(timeInMs, decimalPrecision),
                _y.Evaluate(timeInMs, decimalPrecision)
            );
        }

        public Vector2[] Sample(int count, int decimalPrecision = -1)
        {
            if (count <= 0 || IsEmpty) return Array.Empty<Vector2>();
            var arr = new Vector2[count];
            if (DurationMs <= 0)
            {
                var v = Evaluate(StartMs, decimalPrecision);
                for (int i = 0; i < count; i++) arr[i] = v;
                return arr;
            }

            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0f : (float)i / (count - 1);
                double ms = StartMs + t * DurationMs;
                arr[i] = Evaluate(ms, decimalPrecision);
            }
            return arr;
        }

        public static AnimationCurve2D MakeRamp(Vector2 start, Vector2 end, int durationMs, CurveLoopType loop = CurveLoopType.Constant)
        {
            var c = new AnimationCurve2D(loop);
            c.AddKey(start, 0);
            c.AddKey(end, Math.Max(0, durationMs));
            return c;
        }

        public static AnimationCurve2D MakePulse(Vector2 low, Vector2 high, int upMs, int holdMs, int downMs, CurveLoopType loop = CurveLoopType.Constant)
        {
            var c = new AnimationCurve2D(loop);
            int t0 = 0;
            int t1 = t0 + Math.Max(0, upMs);
            int t2 = t1 + Math.Max(0, holdMs);
            int t3 = t2 + Math.Max(0, downMs);

            c.AddKey(low, t0);
            c.AddKey(high, t1);
            c.AddKey(high, t2);
            c.AddKey(low, t3);
            return c;
        }

        #endregion
    }
}
