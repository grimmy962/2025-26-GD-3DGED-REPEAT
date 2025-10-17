using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;

namespace GDEngine.Core.Components
{
    public class TranslationController : Component
    {
        public float _angularSpeed = 1;
        public float _maxDistance = 1;
        public Vector3 _direction = Vector3.UnitY;
        private Vector3 _originalLocalPosition;

        protected override void Update(float deltaTime)
        {
            var distance 
                = MathF.Sin((float)(Time.RealtimeSinceStartup * _angularSpeed));
            Transform.LocalPosition = _originalLocalPosition 
                + _direction * distance * _maxDistance;
        }
        protected override void Awake()
        {
            _originalLocalPosition = Transform.LocalPosition;
        }
    }
}
