using Microsoft.Xna.Framework;

namespace GDEngine.Core.Timing
{
    /// <summary>
    /// How t value is handled when outside [0,1] when evaluating the curve.
    /// Clamp: clamp to [0,1]. Loop: repeat every 1.0. PingPong: bounce 0→1→0...
    /// </summary>
    public enum CurveWrapMode
    {
        Clamp = 0,
        Loop = 1,
        PingPong = 2
    }

    /// <summary>
    /// Per-segment interpolation between keyframes.
    /// </summary>
    public enum CurveInterpolation
    {
        Constant = 0,   // step
        Linear = 1,     // straight line
        Cubic = 2       // cubic Hermite using tangents
    }

    /// <summary>
    /// A single keyframe defining the curve's shape at a specific time.
    /// Times are expected in [0,1]. Values typically in [0,1], but are not clamped automatically.
    /// Tangents are "value per normalized time" (dy/dt) in curve's 0..1 domain.
    /// </summary>
    public sealed class CurveKey
    {
        #region Fields
        public float Time;         // [0,1]
        public float Value;        // usually [0,1]
        public float InTangent;    // slope entering this key
        public float OutTangent;   // slope exiting this key
        #endregion

        #region Constructors
        /// <summary>
        /// Create a key with explicit in/out tangents.
        /// </summary>
        public CurveKey(float time, float value, float inTangent, float outTangent)
        {
            Time = time;
            Value = value;
            InTangent = inTangent;
            OutTangent = outTangent;
        }

        /// <summary>
        /// Create a key with flat tangents (0 slope).
        /// </summary>
        public CurveKey(float time, float value)
        {
            Time = time;
            Value = value;
            InTangent = 0f;
            OutTangent = 0f;
        }
        #endregion
    }

    /// <summary>
    /// Unity-like AnimationCurve for 0..1 time. Students can define bespoke shapes
    /// and use them as easing functions (e.g., camera, movement, UI tweens).
    /// Supports Constant/Linear/Cubic interpolation and Clamp/Loop/PingPong wrap.
    /// </summary>
    public sealed class AnimationCurve
    {
        #region Static Fields
        #endregion

        #region Fields
        private CurveKey[] _keys = Array.Empty<CurveKey>();
        private int _keyCount;
        private CurveInterpolation _interpolation = CurveInterpolation.Cubic;
        private CurveWrapMode _preWrap = CurveWrapMode.Clamp;
        private CurveWrapMode _postWrap = CurveWrapMode.Clamp;
        #endregion

        #region Properties
        /// <summary>Number of keys currently in the curve.</summary>
        public int KeyCount => _keyCount;

        /// <summary>How segments are interpolated.</summary>
        public CurveInterpolation Interpolation
        {
            get => _interpolation;
            set => _interpolation = value;
        }

        /// <summary>Wrap mode for t &lt; 0.</summary>
        public CurveWrapMode PreWrapMode
        {
            get => _preWrap;
            set => _preWrap = value;
        }

        /// <summary>Wrap mode for t &gt; 1.</summary>
        public CurveWrapMode PostWrapMode
        {
            get => _postWrap;
            set => _postWrap = value;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Empty curve; add keys programmatically, then call <see cref="AutoTangents"/> or <see cref="SmoothTangents(float)"/>.
        /// </summary>
        public AnimationCurve() { }

        /// <summary>
        /// Curve from an initial set of keys (unsorted allowed). Optionally auto-smooth.
        /// </summary>
        public AnimationCurve(CurveKey[] keys, bool smoothTangents = true)
        {
            SetKeys(keys);
            if (smoothTangents) SmoothTangents(0.0f);
        }

        /// <summary>
        /// Prebuild a simple linear 0→1 curve.
        /// </summary>
        public static AnimationCurve Linear01()
        {
            var c = new AnimationCurve();
            c.AddKey(new CurveKey(0f, 0f));
            c.AddKey(new CurveKey(1f, 1f));
            c.Interpolation = CurveInterpolation.Linear;
            return c;
        }

        /// <summary>
        /// Classic ease in-out using a smoothstep curve.
        /// </summary>
        public static AnimationCurve SmoothStep01()
        {
            var c = new AnimationCurve();
            c.AddKey(new CurveKey(0f, 0f));
            c.AddKey(new CurveKey(1f, 1f));
            c.Interpolation = CurveInterpolation.Cubic;
            c.SmoothTangents(0f);
            return c;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Replace all keys.
        /// </summary>
        public void SetKeys(CurveKey[] keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            // Copy
            _keys = new CurveKey[keys.Length];
            for (int i = 0; i < keys.Length; i++)
                _keys[i] = new CurveKey(keys[i].Time, keys[i].Value, keys[i].InTangent, keys[i].OutTangent);

            _keyCount = _keys.Length;
            SortKeys();
        }

        /// <summary>
        /// Add a key and return its index.
        /// </summary>
        public int AddKey(CurveKey key)
        {
            EnsureCapacity(_keyCount + 1);
            _keys[_keyCount] = new CurveKey(key.Time, key.Value, key.InTangent, key.OutTangent);
            _keyCount++;
            SortKeys();
            return IndexOfTime(key.Time);
        }

        /// <summary>
        /// Remove key at index. Returns true if removed.
        /// </summary>
        public bool RemoveKeyAt(int index)
        {
            if (index < 0 || index >= _keyCount)
                return false;

            for (int i = index; i < _keyCount - 1; i++)
                _keys[i] = _keys[i + 1];

            _keyCount--;
            return true;
        }

        /// <summary>
        /// Try get key at index.
        /// </summary>
        public bool TryGetKey(int index, out CurveKey? key)
        {
            if (index < 0 || index >= _keyCount)
            {
                key = null;
                return false;
            }

            key = _keys[index];
            return true;
        }

        /// <summary>
        /// Recompute all tangents using Catmull-Rom style finite differences.
        /// <paramref name="tension"/> in [0,1] tightens the curve (0 = smoothest).
        /// </summary>
        public void SmoothTangents(float tension)
        {
            if (_keyCount < 2)
                return;

            if (tension < 0f) tension = 0f;
            if (tension > 1f) tension = 1f;

            for (int i = 0; i < _keyCount; i++)
            {
                CurveKey prev = i > 0 ? _keys[i - 1] : _keys[i];
                CurveKey next = i < _keyCount - 1 ? _keys[i + 1] : _keys[i];

                float dt = next.Time - prev.Time;
                if (dt <= 0f)
                {
                    _keys[i].InTangent = 0f;
                    _keys[i].OutTangent = 0f;
                }
                else
                {
                    float slope = (next.Value - prev.Value) / dt;
                    float s = slope * (1f - tension);
                    _keys[i].InTangent = s;
                    _keys[i].OutTangent = s;
                }
            }
        }

        /// <summary>
        /// Set all tangents to zero (flat).
        /// </summary>
        public void AutoTangents()
        {
            for (int i = 0; i < _keyCount; i++)
            {
                _keys[i].InTangent = 0f;
                _keys[i].OutTangent = 0f;
            }
        }

        /// <summary>
        /// Evaluate the curve at normalized time t. Applies pre/post wrap and the selected interpolation.
        /// </summary>
        public float Evaluate(float t)
        {
            if (_keyCount == 0)
                return 0f;

            if (_keyCount == 1)
                return _keys[0].Value;

            float tn = ApplyWrap(t);
            int k0 = FindKeyBefore(tn);
            int k1 = k0 + 1;

            // Exact key
            if (Math.Abs(tn - _keys[k0].Time) <= float.Epsilon || k1 >= _keyCount)
                return _keys[k0].Value;

            CurveKey a = _keys[k0];
            CurveKey b = _keys[k1];

            float dt = b.Time - a.Time;
            if (dt <= 0f)
                return a.Value;

            float u = (tn - a.Time) / dt;

            switch (_interpolation)
            {
                case CurveInterpolation.Constant:
                    return a.Value;

                case CurveInterpolation.Linear:
                    return MathHelper.Lerp(a.Value, b.Value, u);

                case CurveInterpolation.Cubic:
                default:
                    // Hermite basis (using a.OutTangent and b.InTangent).
                    float m0 = a.OutTangent * dt;
                    float m1 = b.InTangent * dt;

                    float u2 = u * u;
                    float u3 = u2 * u;

                    float h00 = 2f * u3 - 3f * u2 + 1f;
                    float h10 = u3 - 2f * u2 + u;
                    float h01 = -2f * u3 + 3f * u2;
                    float h11 = u3 - u2;

                    return h00 * a.Value + h10 * m0 + h01 * b.Value + h11 * m1;
            }
        }

        /// <summary>
        /// Convert this curve into a delegate t→value so it can be used anywhere a Func&lt;float,float&gt; is accepted.
        /// </summary>
        public Func<float, float> ToFunc()
        {
            return (x) => Evaluate(x);
        }

        /// <summary>
        /// Ensure internal array fits N items (simple grow-by-doubling).
        /// </summary>
        private void EnsureCapacity(int needed)
        {
            int cap = _keys.Length;
            if (cap >= needed)
                return;

            int newCap = cap == 0 ? 4 : cap * 2;
            while (newCap < needed)
                newCap *= 2;

            var arr = new CurveKey[newCap];
            for (int i = 0; i < _keyCount; i++)
                arr[i] = _keys[i];

            _keys = arr;
        }

        /// <summary>
        /// Sort keys by time ascending (in-place insertion sort—stable for our typical small key counts).
        /// </summary>
        private void SortKeys()
        {
            for (int i = 1; i < _keyCount; i++)
            {
                var k = _keys[i];
                int j = i - 1;
                while (j >= 0 && _keys[j].Time > k.Time)
                {
                    _keys[j + 1] = _keys[j];
                    j--;
                }
                _keys[j + 1] = k;
            }
        }

        /// <summary>
        /// Return the index of the last key with Time ≤ t (clamped into [0, KeyCount-2]).
        /// </summary>
        private int FindKeyBefore(float t)
        {
            // Linear scan is fine for small key counts; can be upgraded to binary search later.
            int last = 0;
            for (int i = 0; i < _keyCount; i++)
            {
                if (_keys[i].Time <= t)
                    last = i;
                else
                    break;
            }

            if (last >= _keyCount - 1)
                last = _keyCount - 2;

            if (last < 0)
                last = 0;

            return last;
        }

        /// <summary>
        /// Return the index of a key at an exact time, else -1.
        /// </summary>
        private int IndexOfTime(float time)
        {
            for (int i = 0; i < _keyCount; i++)
                if (Math.Abs(_keys[i].Time - time) <= float.Epsilon)
                    return i;

            return -1;
        }

        /// <summary>
        /// Apply pre/post wrap to t and return a normalized time in [0,1].
        /// </summary>
        private float ApplyWrap(float t)
        {
            if (t >= 0f && t <= 1f)
                return t;

            if (t < 0f)
                return Wrap(t, _preWrap);

            return Wrap(t, _postWrap);
        }

        /// <summary>
        /// Wrap helper for a given mode.
        /// </summary>
        private float Wrap(float t, CurveWrapMode mode)
        {
            switch (mode)
            {
                case CurveWrapMode.Clamp:
                    if (t < 0f) return 0f;
                    if (t > 1f) return 1f;
                    return t;

                case CurveWrapMode.Loop:
                    // frac for negatives: t - floor(t)
                    {
                        float x = t - (float)Math.Floor(t);
                        return x;
                    }

                case CurveWrapMode.PingPong:
                default:
                    {
                        // Map to 0..2 range, then mirror the >1 region
                        float x = t - (float)Math.Floor(t);
                        // Now x ∈ [0,1) repeating; create 0→1→0 by reflecting every other cycle
                        float twoT = (t - (float)Math.Floor(t)) * 2f;
                        if (twoT <= 1f)
                            return twoT;

                        return 2f - twoT;
                    }
            }
        }
        #endregion
    }
}
