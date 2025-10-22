using Microsoft.Xna.Framework;

namespace GDLibrary.Core.Timing
{
    /// <summary>
    /// Scalar cubic-Hermite animation curve over time (milliseconds).
    /// Rounds optionally, lazily recomputes tangents on mutation, supports sampling and presets.
    /// Looping/clamping/ping-pong behavior is controlled by the <see cref="CurveLoopType"/> you pass to the constructor;
    /// <see cref="Evaluate(double, int)"/> respects that mode automatically.
    /// </summary>
    /// <example>
    /// <code>
    /// // Build a 2-second up-and-down curve (0 -> 1 -> 0) that ping-pongs forever.
    /// // NOTE: Choose the loop type you want here; Evaluate() will honor it automatically.
    /// var curve = new AnimationCurve(CurveLoopType.Oscillate);
    /// curve.AddKey(0f,    0);
    /// curve.AddKey(1f, 1000);
    /// curve.AddKey(0f, 2000);
    ///
    /// // In your update loop (ms domain)
    /// _elapsedMs += (int)(Time.DeltaTime * 1000f);
    /// float y = curve.Evaluate(_elapsedMs); // honors CurveLoopType
    /// </code>
    /// </example>
    /// <see cref="AnimationCurve2D"/>
    /// <see cref="AnimationCurve3D"/>
    public class AnimationCurve
    {
        #region Fields

        private readonly Curve _curve;
        private readonly CurveLoopType _loop;
        private bool _dirtyTangents;

        #endregion

        #region Properties

        /// <summary>Looping behavior applied before/after the key domain and respected by Evaluate().</summary>
        /// <see cref="CurveLoopType"/>
        public CurveLoopType LoopType => _loop;

        /// <summary>Number of keys.</summary>
        public int KeyCount => _curve.Keys.Count;

        /// <summary>True if there are no keys.</summary>
        public bool IsEmpty => _curve.Keys.Count == 0;

        /// <summary>Earliest key time in ms (0 if empty).</summary>
        public int StartMs => IsEmpty ? 0 : (int)_curve.Keys[0].Position;

        /// <summary>Latest key time in ms (0 if empty).</summary>
        public int EndMs => IsEmpty ? 0 : (int)_curve.Keys[_curve.Keys.Count - 1].Position;

        /// <summary>EndMs - StartMs (0 if empty).</summary>
        public int DurationMs => IsEmpty ? 0 : EndMs - StartMs;

        /// <summary>Access for editor visualisation.</summary>
        public CurveKeyCollection Keys => _curve.Keys;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a scalar animation curve (ms domain).
        /// Pass desired loop behavior; Evaluate() will honor it via MonoGame's Curve.
        /// </summary>
        public AnimationCurve(CurveLoopType loopType = CurveLoopType.Cycle)
        {
            _curve = new Curve();
            _curve.PreLoop = _curve.PostLoop = loopType; // let Curve do clamped/cycle/oscillate/etc.
            _loop = loopType;
            _dirtyTangents = true;
        }

        #endregion

        #region Methods

        /// <summary>Add a key at timeInMs.</summary>
        public void AddKey(float value, int timeInMs)
        {
            // If a key already exists at this time, overwrite its value (avoids duplicate-time exceptions)
            for (int i = 0; i < Keys.Count; i++)
            {
                if (Keys[i].Position == timeInMs)
                {
                    var k = Keys[i];
                    k.Value = value;
                    Keys[i] = k;          // update in-place
                    _dirtyTangents = true;
                    return;
                }
            }

            // Otherwise, Add() keeps the collection sorted by Position internally
            Keys.Add(new CurveKey(timeInMs, value));
            _dirtyTangents = true;
        }



        /// <summary>Set the value on an existing key.</summary>
        public bool SetValue(int index, float newValue)
        {
            if (index < 0 || index >= _curve.Keys.Count) return false;
            var k = _curve.Keys[index];
            k.Value = newValue;
            _curve.Keys[index] = k;
            _dirtyTangents = true;
            return true;
        }

        /// <summary>Remove all keys.</summary>
        public void Clear()
        {
            _curve.Keys.Clear();
            _dirtyTangents = true;
        }

        /// <summary>
        /// Evaluate at timeInMs. If decimalPrecision &lt; 0, returns raw value; otherwise rounds for UI display.
        /// Honors the loop/clamp/oscillate behavior provided at construction.
        /// </summary>
        public float Evaluate(double timeInMs, int decimalPrecision = -1)
        {
            if (IsEmpty) return 0f;
            EnsureTangents();

            float v = _curve.Evaluate((float)timeInMs);
            if (decimalPrecision < 0) return v;

            float dp = (float)Math.Pow(10, decimalPrecision);
            return (float)(Math.Round(v * dp) / dp);
        }

        /// <summary>Uniformly sample the curve across [StartMs, EndMs].</summary>
        public float[] Sample(int count, int decimalPrecision = -1)
        {
            if (count <= 0 || IsEmpty) return Array.Empty<float>();
            if (DurationMs <= 0)
            {
                var v = Evaluate(StartMs, decimalPrecision);
                var arr = new float[count];
                for (int i = 0; i < count; i++) arr[i] = v;
                return arr;
            }

            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0f : (float)i / (count - 1);
                double ms = StartMs + t * DurationMs;
                data[i] = Evaluate(ms, decimalPrecision);
            }
            return data;
        }

        /// <summary>Get min/max across key values (fast heuristic for UI scaling).</summary>
        public bool TryGetValueRange(out float min, out float max)
        {
            min = 0f; max = 0f;
            if (IsEmpty) return false;

            min = float.PositiveInfinity;
            max = float.NegativeInfinity;

            for (int i = 0; i < _curve.Keys.Count; i++)
            {
                float v = _curve.Keys[i].Value;
                if (v < min) min = v;
                if (v > max) max = v;
            }
            return true;
        }

        /// <summary>Create a simple linear ramp from start to end over durationMs.</summary>
        public static AnimationCurve MakeRamp(float startValue, float endValue, int durationMs, CurveLoopType loop = CurveLoopType.Constant)
        {
            var c = new AnimationCurve(loop);
            c.AddKey(startValue, 0);
            c.AddKey(endValue, Math.Max(0, durationMs));
            return c;
        }

        /// <summary>Create a pulse (low→high→low) with up/hold/down segments (ms).</summary>
        public static AnimationCurve MakePulse(float low, float high, int upMs, int holdMs, int downMs, CurveLoopType loop = CurveLoopType.Constant)
        {
            var c = new AnimationCurve(loop);
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

        #region Lifecycle Methods

        private void EnsureTangents()
        {
            if (!_dirtyTangents) return;
            if (IsEmpty) return;

            for (int i = 0; i < _curve.Keys.Count; i++)
                ComputeTangentsForKey(i);

            _dirtyTangents = false;
        }

        private void ComputeTangentsForKey(int i)
        {
            // Neighbour indices (non-cyclic for better endpoints; use self when missing)
            int prev = i - 1; if (prev < 0) prev = i;
            int next = i + 1; if (next >= _curve.Keys.Count) next = i;

            var kPrev = _curve.Keys[prev];
            var k = _curve.Keys[i];
            var kNext = _curve.Keys[next];

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

            _curve.Keys[i] = k;
        }

        #endregion

        #region Housekeeping Methods

        public override string ToString()
        {
            return $"AnimationCurve(Keys={KeyCount}, StartMs={StartMs}, EndMs={EndMs}, Loop={_loop})";
        }

        #endregion
    }
}
