using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace GDEngine.Core.Components
{
    public class MouseYawPitchController : Component
    {
        private MouseState _newMouseState;
        private MouseState _oldMouseState;
        private float _mouseSensitivity = 0.1f;

        protected override void Update(float deltaTime)
        {
            if (Transform == null)
                return;

            //get mouse state
            _newMouseState = Mouse.GetState();

            //get x delta
            float dX = _newMouseState.X - _oldMouseState.X;

            dX *= _mouseSensitivity;
            dX += Time.DeltaTime;

            //apply to rotation around Up
            var yawQuaternion = Quaternion.CreateFromAxisAngle(
                Vector3.Up, dX);

            Transform.Rotate(yawQuaternion, true);

            //store old state for delta calculation
            _oldMouseState = _newMouseState;
        }

    }
}
