using Microsoft.Xna.Framework;

namespace GDEngine.Core.Timing
{
    /// <remarks>
    /// Scalar animation curve (SECONDS).
    /// Evaluate() honors the chosen CurveLoopType.    /// Lazy tangent recomputation on mutation. Sampling helpers and simple presets.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Build a 2-second up-and-down curve (0 -> 1 -> 0) that oscillates (ping-pongs).
    /// var curve = new AnimationCurve(); // default = Cycle; use Oscillate if you want ping-pong
    /// curve.AddKey(0f, 0.0);  // (value, timeSeconds)
    /// curve.AddKey(1f, 1.0);
    /// curve.AddKey(0f, 2.0);
    ///
    /// double elapsedSeconds = 0.0;
    /// elapsedSeconds += Time.DeltaTimeSecs;
    /// float y = curve.Evaluate(elapsedSeconds); // honors CurveLoopType
    ///
    /// // Sampling (e.g., for graphing UI)
    /// float[] samples = curve.Sample(32);
    /// </code>
    /// </example>
    /// <see cref="AnimationCurve2D"/>
    /// <see cref="AnimationCurve3D"/>
    public class AnimationCurve
    {
        // Fields
        private readonly Curve _curve;
        private readonly CurveLoopType _loop;
        private bool _dirtyTangents;

        // Properties
        public CurveLoopType LoopType => _loop;                    // loop/clamp/oscillate behavior honored by Evaluate()
        public int KeyCount => _curve.Keys.Count;                  // number of keys
        public bool IsEmpty => _curve.Keys.Count == 0;             // true if there are no keys
        public double StartSeconds => IsEmpty ? 0.0 : _curve.Keys[0].Position;
        public double EndSeconds => IsEmpty ? 0.0 : _curve.Keys[_curve.Keys.Count - 1].Position;
        public double DurationSeconds => IsEmpty ? 0.0 : EndSeconds - StartSeconds;
        public CurveKeyCollection Keys => _curve.Keys;             // direct access to MonoGame keys

        // Constructor (defaults to Cycle)
        public AnimationCurve(CurveLoopType loopType = CurveLoopType.Cycle)
        {
            _curve = new Curve();
            _curve.PreLoop = _curve.PostLoop = loopType;
            _loop = loopType;
            _dirtyTangents = true;
        }

        // Add a key at timeSeconds. If a key exists at the same time, overwrite its value.
        public void AddKey(float value, double timeSeconds)
        {
            float t = (float)timeSeconds;

            for (int i = 0; i < Keys.Count; i++)
            {
                if (Keys[i].Position == t)
                {
                    var k = Keys[i];
                    k.Value = value;
                    Keys[i] = k;
                    _dirtyTangents = true;
                    return;
                }
            }

            Keys.Add(new CurveKey(t, value)); // MonoGame keeps Keys sorted by Position
            _dirtyTangents = true;
        }

        // Set the value on an existing key by index.
        public bool SetValue(int index, float newValue)
        {
            if (index < 0 || index >= Keys.Count) return false;
            var k = Keys[index];
            k.Value = newValue;
            Keys[index] = k;
            _dirtyTangents = true;
            return true;
        }

        // Remove all keys.
        public void Clear()
        {
            Keys.Clear();
            _dirtyTangents = true;
        }

        // Evaluate at timeSeconds. If decimalPrecision < 0, returns raw value; otherwise rounds for UI display.
        // Respects the loop/clamp/oscillate behavior set at construction.
        public float Evaluate(double timeSeconds, int decimalPrecision = -1)
        {
            if (IsEmpty) return 0f;
            EnsureTangents();

            float v = _curve.Evaluate((float)timeSeconds);
            if (decimalPrecision < 0) return v;

            float dp = (float)Math.Pow(10, decimalPrecision);
            return (float)(Math.Round(v * dp) / dp);
        }

        // Uniformly sample the curve across [StartSeconds, EndSeconds].
        public float[] Sample(int count, int decimalPrecision = -1)
        {
            if (count <= 0 || IsEmpty) return Array.Empty<float>();
            if (DurationSeconds <= 0.0)
            {
                var v = Evaluate(StartSeconds, decimalPrecision);
                var arr = new float[count];
                for (int i = 0; i < count; i++) arr[i] = v;
                return arr;
            }

            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t01 = count == 1 ? 0f : (float)i / (count - 1);
                double s = StartSeconds + t01 * DurationSeconds;
                data[i] = Evaluate(s, decimalPrecision);
            }
            return data;
        }

        // Get min/max across key values (fast heuristic for UI scaling).
        public bool TryGetValueRange(out float min, out float max)
        {
            min = 0f; max = 0f;
            if (IsEmpty) return false;

            min = float.PositiveInfinity;
            max = float.NegativeInfinity;

            for (int i = 0; i < Keys.Count; i++)
            {
                float v = Keys[i].Value;
                if (v < min) min = v;
                if (v > max) max = v;
            }
            return true;
        }

        // Create a simple linear ramp from startValue to endValue over durationSeconds.
        public static AnimationCurve MakeRamp(float startValue, double durationSeconds, float endValue, CurveLoopType loop = CurveLoopType.Cycle)
        {
            var c = new AnimationCurve(loop);
            c.AddKey(startValue, 0.0);
            c.AddKey(endValue, Math.Max(0.0, durationSeconds));
            return c;
        }

        // Create a pulse (low→high→low) with up/hold/down segments (seconds).
        public static AnimationCurve MakePulse(float low, float high, double upSeconds, double holdSeconds, double downSeconds, CurveLoopType loop = CurveLoopType.Cycle)
        {
            var c = new AnimationCurve(loop);
            double t0 = 0.0;
            double t1 = t0 + Math.Max(0.0, upSeconds);
            double t2 = t1 + Math.Max(0.0, holdSeconds);
            double t3 = t2 + Math.Max(0.0, downSeconds);

            c.AddKey(low, t0);
            c.AddKey(high, t1);
            c.AddKey(high, t2);
            c.AddKey(low, t3);
            return c;
        }

        // Tangent maintenance
        private void EnsureTangents()
        {
            if (!_dirtyTangents) return;
            if (IsEmpty) return;

            for (int i = 0; i < Keys.Count; i++)
                ComputeTangentsForKey(i);

            _dirtyTangents = false;
        }

        // Compute Catmull-Rom–style finite-difference tangents for key i.
        private void ComputeTangentsForKey(int i)
        {
            int prev = i - 1; if (prev < 0) prev = i;
            int next = i + 1; if (next >= Keys.Count) next = i;

            var kPrev = Keys[prev];
            var k = Keys[i];
            var kNext = Keys[next];

            float dtPrev = k.Position - kPrev.Position;
            float dtNext = kNext.Position - k.Position;

            float slopeIn, slopeOut;

            if (i == prev && i == next)
            {
                slopeIn = slopeOut = 0f;
            }
            else if (i == prev)
            {
                slopeIn = slopeOut = dtNext != 0 ? (kNext.Value - k.Value) / dtNext : 0f;
            }
            else if (i == next)
            {
                slopeIn = slopeOut = dtPrev != 0 ? (k.Value - kPrev.Value) / dtPrev : 0f;
            }
            else
            {
                float dt = kNext.Position - kPrev.Position;
                float dv = kNext.Value - kPrev.Value;
                float m = dt != 0 ? dv / dt : 0f;
                slopeIn = m;
                slopeOut = m;
            }

            k.TangentIn = slopeIn;
            k.TangentOut = slopeOut;

            Keys[i] = k;
        }

        public override string ToString()
        {
            return $"AnimationCurve(Keys={KeyCount}, Start={StartSeconds:F3}s, End={EndSeconds:F3}s, Loop={_loop})";
        }
    }

    /// <remarks>
    /// 2D (x,y) animation curve composed of two scalar curves sharing the same time domain (SECONDS).
    /// Evaluate() honors the chosen CurveLoopType.
    /// </remarks>
    /// <example>
    /// <code>
    /// var curve2 = new AnimationCurve2D(); // default = Cycle
    /// curve2.AddKey(new Vector2(0, 0),  0.0);
    /// curve2.AddKey(new Vector2(4, 2),  1.5);
    /// curve2.AddKey(new Vector2(0,-2),  3.0);
    ///
    /// double elapsedSeconds = 0.0;
    /// elapsedSeconds += Time.DeltaTimeSecs;
    /// Vector2 p = curve2.Evaluate(elapsedSeconds);
    ///
    /// Vector2[] pts = curve2.Sample(50);
    /// </code>
    /// </example>
    /// <see cref="AnimationCurve"/>
    /// <see cref="AnimationCurve3D"/>
    public class AnimationCurve2D
    {
        // Fields
        private readonly AnimationCurve _x;
        private readonly AnimationCurve _y;

        // Properties
        public CurveLoopType LoopType => _x.LoopType;
        public int KeyCount => Math.Max(_x.KeyCount, _y.KeyCount);
        public bool IsEmpty => _x.IsEmpty && _y.IsEmpty;

        public double StartSeconds
        {
            get
            {
                double sx = _x.IsEmpty ? double.PositiveInfinity : _x.StartSeconds;
                double sy = _y.IsEmpty ? double.PositiveInfinity : _y.StartSeconds;
                double s = Math.Min(sx, sy);
                return double.IsPositiveInfinity(s) ? 0.0 : s;
            }
        }

        public double EndSeconds => Math.Max(_x.EndSeconds, _y.EndSeconds);
        public double DurationSeconds => IsEmpty ? 0.0 : EndSeconds - StartSeconds;

        // Constructor (defaults to Cycle)
        public AnimationCurve2D(CurveLoopType loopType = CurveLoopType.Cycle)
        {
            _x = new AnimationCurve(loopType);
            _y = new AnimationCurve(loopType);
        }

        // Add a 2D key at timeSeconds.
        public void AddKey(Vector2 value, double timeSeconds)
        {
            _x.AddKey(value.X, timeSeconds);
            _y.AddKey(value.Y, timeSeconds);
        }

        // Set both components on an existing key index.
        public bool SetValue(int index, Vector2 newValue)
        {
            bool a = _x.SetValue(index, newValue.X);
            bool b = _y.SetValue(index, newValue.Y);
            return a || b;
        }

        // Remove all keys.
        public void Clear()
        {
            _x.Clear();
            _y.Clear();
        }

        // Evaluate at timeSeconds.
        public Vector2 Evaluate(double timeSeconds, int decimalPrecision = -1)
        {
            return new Vector2(
                _x.Evaluate(timeSeconds, decimalPrecision),
                _y.Evaluate(timeSeconds, decimalPrecision)
            );
        }

        // Uniformly sample across [StartSeconds, EndSeconds].
        public Vector2[] Sample(int count, int decimalPrecision = -1)
        {
            if (count <= 0 || IsEmpty) return Array.Empty<Vector2>();
            var arr = new Vector2[count];
            if (DurationSeconds <= 0.0)
            {
                var v = Evaluate(StartSeconds, decimalPrecision);
                for (int i = 0; i < count; i++) arr[i] = v;
                return arr;
            }

            for (int i = 0; i < count; i++)
            {
                float t01 = count == 1 ? 0f : (float)i / (count - 1);
                double s = StartSeconds + t01 * DurationSeconds;
                arr[i] = Evaluate(s, decimalPrecision);
            }
            return arr;
        }

        // Factory: linear ramp over durationSeconds.
        public static AnimationCurve2D MakeRamp(Vector2 start, double durationSeconds, Vector2 end, CurveLoopType loop = CurveLoopType.Cycle)
        {
            var c = new AnimationCurve2D(loop);
            c.AddKey(start, 0.0);
            c.AddKey(end, Math.Max(0.0, durationSeconds));
            return c;
        }

        // Factory: pulse (low→high→low) segments in seconds.
        public static AnimationCurve2D MakePulse(Vector2 low, Vector2 high, double upSeconds, double holdSeconds, double downSeconds, CurveLoopType loop = CurveLoopType.Cycle)
        {
            var c = new AnimationCurve2D(loop);
            double t0 = 0.0;
            double t1 = t0 + Math.Max(0.0, upSeconds);
            double t2 = t1 + Math.Max(0.0, holdSeconds);
            double t3 = t2 + Math.Max(0.0, downSeconds);

            c.AddKey(low, t0);
            c.AddKey(high, t1);
            c.AddKey(high, t2);
            c.AddKey(low, t3);
            return c;
        }
    }

    /// <remarks>
    /// 3D (x,y,z) animation curve composed of three scalar curves sharing the same time domain (SECONDS).
    /// Evaluate() honors the chosen CurveLoopType.
    /// </remarks>
    /// <example>
    /// <code>
    /// var path = new AnimationCurve3D(); // default = Cycle
    /// path.AddKey(new Vector3(0, 1,  0), 0.0);
    /// path.AddKey(new Vector3(4, 2, -5), 1.5);
    /// path.AddKey(new Vector3(0, 1,-10), 3.0);
    ///
    /// double elapsedSeconds = 0.0;
    /// elapsedSeconds += Time.DeltaTimeSecs;
    /// Vector3 camPos = path.Evaluate(elapsedSeconds);
    ///
    /// Vector3[] pts = path.Sample(64);
    /// </code>
    /// </example>
    /// <see cref="AnimationCurve"/>
    /// <see cref="AnimationCurve2D"/>
    public class AnimationCurve3D
    {
        // Fields
        private readonly AnimationCurve _x;
        private readonly AnimationCurve _y;
        private readonly AnimationCurve _z;

        // Properties
        public CurveLoopType LoopType => _x.LoopType;
        public int KeyCount => Math.Max(_x.KeyCount, Math.Max(_y.KeyCount, _z.KeyCount));
        public bool IsEmpty => _x.IsEmpty && _y.IsEmpty && _z.IsEmpty;

        public double StartSeconds
        {
            get
            {
                double sx = _x.IsEmpty ? double.PositiveInfinity : _x.StartSeconds;
                double sy = _y.IsEmpty ? double.PositiveInfinity : _y.StartSeconds;
                double sz = _z.IsEmpty ? double.PositiveInfinity : _z.StartSeconds;
                double s = Math.Min(sx, Math.Min(sy, sz));
                return double.IsPositiveInfinity(s) ? 0.0 : s;
            }
        }

        public double EndSeconds => Math.Max(_x.EndSeconds, Math.Max(_y.EndSeconds, _z.EndSeconds));
        public double DurationSeconds => IsEmpty ? 0.0 : EndSeconds - StartSeconds;

        // Constructor (defaults to Cycle)
        public AnimationCurve3D(CurveLoopType loopType = CurveLoopType.Cycle)
        {
            _x = new AnimationCurve(loopType);
            _y = new AnimationCurve(loopType);
            _z = new AnimationCurve(loopType);
        }

        // Add a 3D key at timeSeconds.
        public void AddKey(Vector3 value, double timeSeconds)
        {
            _x.AddKey(value.X, timeSeconds);
            _y.AddKey(value.Y, timeSeconds);
            _z.AddKey(value.Z, timeSeconds);
        }

        // Set all three components on an existing key index.
        public bool SetValue(int index, Vector3 newValue)
        {
            bool a = _x.SetValue(index, newValue.X);
            bool b = _y.SetValue(index, newValue.Y);
            bool c = _z.SetValue(index, newValue.Z);
            return a || b || c;
        }

        // Remove all keys.
        public void Clear()
        {
            _x.Clear();
            _y.Clear();
            _z.Clear();
        }

        // Evaluate at timeSeconds.
        public Vector3 Evaluate(double timeSeconds, int decimalPrecision = -1)
        {
            return new Vector3(
                _x.Evaluate(timeSeconds, decimalPrecision),
                _y.Evaluate(timeSeconds, decimalPrecision),
                _z.Evaluate(timeSeconds, decimalPrecision)
            );
        }

        // Uniformly sample across [StartSeconds, EndSeconds].
        public Vector3[] Sample(int count, int decimalPrecision = -1)
        {
            if (count <= 0 || IsEmpty) return Array.Empty<Vector3>();
            var arr = new Vector3[count];
            if (DurationSeconds <= 0.0)
            {
                var v = Evaluate(StartSeconds, decimalPrecision);
                for (int i = 0; i < count; i++) arr[i] = v;
                return arr;
            }

            for (int i = 0; i < count; i++)
            {
                float t01 = count == 1 ? 0f : (float)i / (count - 1);
                double s = StartSeconds + t01 * DurationSeconds;
                arr[i] = Evaluate(s, decimalPrecision);
            }
            return arr;
        }

        // Factory: linear ramp over durationSeconds.
        public static AnimationCurve3D MakeRamp(Vector3 start, double durationSeconds, Vector3 end, CurveLoopType loop = CurveLoopType.Cycle)
        {
            var c = new AnimationCurve3D(loop);
            c.AddKey(start, 0.0);
            c.AddKey(end, Math.Max(0.0, durationSeconds));
            return c;
        }

        // Factory: pulse (low→high→low) segments in seconds.
        public static AnimationCurve3D MakePulse(Vector3 low, Vector3 high, double upSeconds, double holdSeconds, double downSeconds, CurveLoopType loop = CurveLoopType.Cycle)
        {
            var c = new AnimationCurve3D(loop);
            double t0 = 0.0;
            double t1 = t0 + Math.Max(0.0, upSeconds);
            double t2 = t1 + Math.Max(0.0, holdSeconds);
            double t3 = t2 + Math.Max(0.0, downSeconds);

            c.AddKey(low, t0);
            c.AddKey(high, t1);
            c.AddKey(high, t2);
            c.AddKey(low, t3);
            return c;
        }
    }
}
