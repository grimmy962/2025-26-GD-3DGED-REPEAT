using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;

namespace GDEngine.Core.Components
{
    public class DemoAnimationCurveController : Component
    {
        #region Fields
        private Vector3 _direction = Vector3.UnitY;
        private AnimationCurve _curve;

        //cached vars
        private float _totalElapsedTimeSecs;
        private Vector3 _originalLocalPosition;

        public DemoAnimationCurveController(AnimationCurve curve)
        {
            _curve = curve;
        }
        #endregion

        public Vector3 Direction { get => _direction; set => _direction = value; }
        public AnimationCurve Curve { get => _curve; set => _curve = value; }

        protected override void Update(float deltaTime)
        {
            if (Curve == null)
                throw new ArgumentNullException(nameof(Curve));

            _totalElapsedTimeSecs += Time.UnscaledDeltaTimeSecs;
            
           var delta = _curve.Evaluate(_totalElapsedTimeSecs, 2);
           Transform?.TranslateTo(_originalLocalPosition + delta * _direction);
        }

        protected override void Awake()
        {
            if (Transform == null)
                throw new ArgumentNullException(nameof(Transform));

            //store so we always apply curve output to original position
            _originalLocalPosition = Transform.LocalPosition;

            //remove any scale on direction so its pure curve driven movement
            _direction.Normalize();
        }
    }
}
