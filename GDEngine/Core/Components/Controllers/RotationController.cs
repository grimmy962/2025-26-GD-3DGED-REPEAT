using GDEngine.Core.Entities;
using Microsoft.Xna.Framework;

namespace GDEngine.Core.Components
{
    /// <summary>
    /// Rotates its <see cref="GameObject"/> around a configurable local axis at a configurable angular speed.
    /// </summary>
    /// <see cref="Transform"/>
    public sealed class RotationController : Component
    {
        #region Fields
        // Rotation axis in local space. Will be normalized at runtime.
        public Vector3 _rotationAxisNormalized = Vector3.Up;

        // Rotation speed in degrees per second.
        public float _rotationSpeedInRadiansPerSecond = (float)Math.PI/2;

        private static readonly float ROTATION_THRESHOLD = 1E-8f;
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// Applies a delta rotation around <see cref="_rotationAxisNormalized"/> each frame.
        /// </summary>
        /// <param name="deltaTime">Seconds since last frame.</param>
        protected override void Update(float deltaTime)
        {
            if (!Enabled)
                return;

            if (MathF.Abs(_rotationSpeedInRadiansPerSecond) 
                                        <= ROTATION_THRESHOLD)
                return;

            float angle = _rotationSpeedInRadiansPerSecond * deltaTime;
            Quaternion rotQuaternion = Quaternion.CreateFromAxisAngle(_rotationAxisNormalized, angle);

            //the ? means only call if Transform is not null (it's like wrapping in an if(Transform != null) clause)
            Transform.LocalRotation = Quaternion.Normalize(rotQuaternion * Transform.LocalRotation);

        }

        protected override void Awake()
        {       
            //just in case there's anything wrong with Transform
            if (Transform == null)
                throw new ArgumentNullException(nameof(Transform));

            //just in case user enters something with greater than |1| length
            _rotationAxisNormalized.Normalize();

            //NO-OP
            //base.Awake();
        }
        #endregion
    }
}
