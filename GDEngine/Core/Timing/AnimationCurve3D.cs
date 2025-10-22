using Microsoft.Xna.Framework;

namespace GDLibrary.Core.Timing
{
    /// <summary>
    /// 3D (x,y,z) animation curve composed of three scalar curves sharing the same time domain.
    /// Looping/clamping/ping-pong behavior is taken from the supplied <see cref="CurveLoopType"/> and respected by <see cref="Evaluate(double, int)"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// // Animate a camera path over 3 seconds; Evaluate() honors the loop type.
    /// var path = new AnimationCurve3D(CurveLoopType.Cycle);
    /// path.AddKey(new Vector3(0, 1,  0),     0);
    /// path.AddKey(new Vector3(4, 2, -5),  1500);
    /// path.AddKey(new Vector3(0, 1,-10),  3000);
    ///
    /// _elapsedMs += (int)(Time.DeltaTime * 1000f);
    /// Vector3 camPos = path.Evaluate(_elapsedMs); // honors CurveLoopType
    ///
    /// </code>
    /// </example>
    /// <see cref="AnimationCurve"/>
    /// <see cref="AnimationCurve2D"/>
    public class AnimationCurve3D
    {
        #region Fields

        private readonly AnimationCurve _x;
        private readonly AnimationCurve _y;
        private readonly AnimationCurve _z;

        #endregion

        #region Properties

        public CurveLoopType LoopType => _x.LoopType;

        public int KeyCount => Math.Max(_x.KeyCount, Math.Max(_y.KeyCount, _z.KeyCount));

        public bool IsEmpty => _x.IsEmpty && _y.IsEmpty && _z.IsEmpty;

        public int StartMs
        {
            get
            {
                int sx = _x.IsEmpty ? int.MaxValue : _x.StartMs;
                int sy = _y.IsEmpty ? int.MaxValue : _y.StartMs;
                int sz = _z.IsEmpty ? int.MaxValue : _z.StartMs;
                int s = Math.Min(sx, Math.Min(sy, sz));
                return s == int.MaxValue ? 0 : s;
            }
        }

        public int EndMs => Math.Max(_x.EndMs, Math.Max(_y.EndMs, _z.EndMs));

        public int DurationMs => IsEmpty ? 0 : EndMs - StartMs;

        #endregion

        #region Constructors

        public AnimationCurve3D(CurveLoopType loopType)
        {
            _x = new AnimationCurve(loopType);
            _y = new AnimationCurve(loopType);
            _z = new AnimationCurve(loopType);
        }

        #endregion

        #region Methods

        public void AddKey(Vector3 value, int timeInMs)
        {
            _x.AddKey(value.X, timeInMs);
            _y.AddKey(value.Y, timeInMs);
            _z.AddKey(value.Z, timeInMs);
        }

        public bool SetValue(int index, Vector3 newValue)
        {
            bool a = _x.SetValue(index, newValue.X);
            bool b = _y.SetValue(index, newValue.Y);
            bool c = _z.SetValue(index, newValue.Z);
            return a || b || c;
        }

        public void Clear()
        {
            _x.Clear();
            _y.Clear();
            _z.Clear();
        }

        public Vector3 Evaluate(double timeInMs, int decimalPrecision = -1)
        {
            return new Vector3(
                _x.Evaluate(timeInMs, decimalPrecision),
                _y.Evaluate(timeInMs, decimalPrecision),
                _z.Evaluate(timeInMs, decimalPrecision)
            );
        }

        public Vector3[] Sample(int count, int decimalPrecision = -1)
        {
            if (count <= 0 || IsEmpty) return Array.Empty<Vector3>();
            var arr = new Vector3[count];
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

        public static AnimationCurve3D MakeRamp(Vector3 start, Vector3 end, int durationMs, CurveLoopType loop = CurveLoopType.Constant)
        {
            var c = new AnimationCurve3D(loop);
            c.AddKey(start, 0);
            c.AddKey(end, Math.Max(0, durationMs));
            return c;
        }

        public static AnimationCurve3D MakePulse(Vector3 low, Vector3 high, int upMs, int holdMs, int downMs, CurveLoopType loop = CurveLoopType.Constant)
        {
            var c = new AnimationCurve3D(loop);
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
