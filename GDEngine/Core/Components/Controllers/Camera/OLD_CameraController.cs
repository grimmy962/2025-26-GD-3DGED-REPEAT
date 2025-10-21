using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDEngine.Core.Components
{
    public class OLD_CameraController : Component
    {
        private KeyboardState _kbState;
        protected override void LateUpdate(float deltaTime)
        {
            if (Transform == null)
                return;

            _kbState = Keyboard.GetState();

            if (_kbState.IsKeyDown(Keys.W))
                Transform.TranslateBy(0.1f * Transform.Forward);
            else if (_kbState.IsKeyDown(Keys.S))
                Transform.TranslateBy(-0.1f * Transform.Forward);

            if (_kbState.IsKeyDown(Keys.A))
                Transform.TranslateBy(0.1f * Transform.Right);
            else if (_kbState.IsKeyDown(Keys.D))
                Transform.TranslateBy(-0.1f * Transform.Right);


            if (_kbState.IsKeyDown(Keys.Q))
                Transform.Transform.RotateEuler(
                new Vector3(MathHelper.ToRadians(1), 0, 0));
            else if (_kbState.IsKeyDown(Keys.E))
                Transform.Transform.RotateEuler(
                new Vector3(MathHelper.ToRadians(-1), 0, 0));

        }
    }
}


