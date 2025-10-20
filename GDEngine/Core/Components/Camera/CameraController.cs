using Microsoft.Xna.Framework.Input;
using System;

namespace GDEngine.Core.Components
{
    public class CameraController : Component
    {
        private KeyboardState _kbState;
        protected override void LateUpdate(float deltaTime)
        {
            if (Transform == null)
                return;

            if (_kbState.IsKeyDown(Keys.W))
                Transform.TranslateBy(0.1f * Transform.Forward);
            else if (_kbState.IsKeyDown(Keys.S))
                Transform.TranslateBy(-0.1f * Transform.Forward);
        }

        protected override void Awake()
        {
            _kbState = Keyboard.GetState();
        }
    }
}
