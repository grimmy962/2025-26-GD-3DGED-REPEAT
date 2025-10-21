using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDEngine.Core.Components
{
    public class MouseYawPitchController : Component
    {
        private MouseState _newMouseState;
        private MouseState _oldMouseState;
        private float _mouseSensitivity = 0.001f;

        protected override void Update(float deltaTime)
        {
            //get mouse state
            _newMouseState = Mouse.GetState();

            //get x delta
            float ds = _newMouseState.X - _oldMouseState.X;

            ds *= _mouseSensitivity;
            ds += Time.DeltaTime;

            //apply to rotation around Up
            var yawQuaternion = Quaternion.CreateFromAxisAngle(
                Vector3.Up, ds);

            Transform.Rotate(yawQuaternion, true);

            //store old state for delta calculation
            _oldMouseState = _newMouseState;
        }
    }
}
