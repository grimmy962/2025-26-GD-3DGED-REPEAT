using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDEngine.Core.Components
{
    /// <summary>
    /// Simple drive controller:
    /// U/J = move forward/back along current facing; H/K = yaw left/right.
    /// </summary>
    /// <see cref="Component"/>
    /// <see cref="Transform"/>
    public sealed class SimpleDriveController : Component
    {
        #region Fields
        private float _moveSpeed = 15f;   // units/sec
        private float _turnSpeed = 5f; // radians/sec
        #endregion

        #region Lifecycle Methods
        protected override void Update(float deltaTime)
        {
            if (Transform == null)
                return;

            var k = Keyboard.GetState();

            // Yaw (H left, K right)
            float yaw = 0f;
            if (k.IsKeyDown(Keys.H)) yaw += 1f;
            if (k.IsKeyDown(Keys.K)) yaw -= 1f;

            if (yaw != 0f)
                Transform.RotateEulerBy(new Vector3(0f, yaw * _turnSpeed * deltaTime, 0f));

            // Forward/back (U forward, J back)
            float fwd = 0f;
            if (k.IsKeyDown(Keys.U)) fwd -= 1f;
            if (k.IsKeyDown(Keys.J)) fwd += 1f;

            if (fwd != 0f)
            {
                Vector3 dir = Transform.Forward; // now correct after Transform fix
                Vector3 worldDelta = -dir * (fwd * _moveSpeed * deltaTime);
                Transform.TranslateBy(worldDelta, worldSpace: true);
            }
        }
        #endregion
    }
}
