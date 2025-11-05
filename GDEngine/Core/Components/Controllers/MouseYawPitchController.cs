using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDEngine.Core.Components
{
    public class MouseYawPitchController : Component
    {
        private MouseState _newMouseState;
        private MouseState _oldMouseState;
        private float _mouseSensitivity = 0.4f;

        protected override void Awake()
        {
            _oldMouseState = Mouse.GetState();
        }

        protected override void Update(float deltaTime)
        {
            if (Transform == null)
                return;

            //get mouse state
            _newMouseState = Mouse.GetState();

            //get x delta
            float dX = _newMouseState.X - _oldMouseState.X;

            dX *= _mouseSensitivity;
            dX *= Time.DeltaTimeSecs;

            float dY = _newMouseState.Y - _oldMouseState.Y;
            dY *= _mouseSensitivity * 0.5f;
            dY *= Time.DeltaTimeSecs;

            //apply to rotation around Up
            var yawQuaternion = Quaternion.CreateFromAxisAngle(Vector3.Up, dX);
            Transform.RotateBy(yawQuaternion, true);

            var pitchRotation = Quaternion.CreateFromAxisAngle(Transform.Right, -dY);
            Transform.RotateBy(pitchRotation, true);

            //store old state for delta calculation
            _oldMouseState = _newMouseState;
        }

    }
}
