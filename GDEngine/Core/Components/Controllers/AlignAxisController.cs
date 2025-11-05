using GDEngine.Core.Systems;
using Microsoft.Xna.Framework;

namespace GDEngine.Core.Components
{
    /// <summary>
    /// Matches a GameObject's rotation to the active camera every frame,
    /// using a stable delta-quaternion converted to yaw/pitch/roll and applied
    /// via <c>Transform.RotateEulerBy(..., worldSpace:true)</c>.
    /// </summary>
    /// <see cref="Component"/>
    /// <see cref="Transform"/>
    /// <see cref="CameraSystem"/>
    public sealed class AlignAxisController : Component
    {
        #region Fields
        private CameraSystem? _cameraSystem;
        #endregion

        #region Methods
        // Quaternion -> (pitch, yaw, roll) in radians, compatible with CreateFromYawPitchRoll(y, x, z).
        private static Vector3 ToYawPitchRoll(in Quaternion q)
        {
            var m = Matrix.CreateFromQuaternion(q);

            float pitch = (float)System.Math.Asin(System.Math.Clamp(-m.M32, -1f, 1f)); // X
            float cp = (float)System.Math.Cos(pitch);

            float yaw, roll;
            if (Math.Abs(cp) > 1e-5f)
            {
                yaw = (float)System.Math.Atan2(m.M31 / cp, m.M33 / cp); // Y
                roll = (float)System.Math.Atan2(m.M12 / cp, m.M22 / cp); // Z
            }
            else
            {
                // Gimbal-ish fallback
                yaw = 0f;
                roll = (float)System.Math.Atan2(-m.M21, m.M11);
            }

            // Return as (pitch, yaw, roll) so X=pitch, Y=yaw, Z=roll.
            return new Vector3(pitch, yaw, roll);
        }

        // Wrap to (-π, π]
        private static float WrapAngle(float r)
        {
            while (r <= -MathF.PI) r += MathF.Tau;
            while (r > MathF.PI) r -= MathF.Tau;
            return r;
        }

        private void MatchCameraRotation()
        {
            if (_cameraSystem == null || _cameraSystem.ActiveCamera == null || Transform == null)
                return;

            // World-space current & target
            Quaternion currentW = Transform.Rotation;
            Quaternion targetW = _cameraSystem.ActiveCamera.Transform.Rotation;

            // World delta: rotate current -> target
            Quaternion deltaW = Quaternion.Normalize(Quaternion.Concatenate(
                                targetW, Quaternion.Inverse(currentW)));

            // Ensure shortest-arc representation (quats q and -q are the same rotation).
            if (deltaW.W < 0f)
            {
                deltaW.X = -deltaW.X; deltaW.Y = -deltaW.Y; deltaW.Z = -deltaW.Z; deltaW.W = -deltaW.W;
            }

            // Convert the *delta* quaternion to YPR (small angles) and wrap.
            Vector3 deltaYPR = ToYawPitchRoll(deltaW);
            deltaYPR.X = WrapAngle(deltaYPR.X);
            deltaYPR.Y = WrapAngle(deltaYPR.Y);
            deltaYPR.Z = WrapAngle(deltaYPR.Z);

            // Apply that delta in WORLD space.
            Transform.RotateEulerBy(deltaYPR, worldSpace: true);
        }
        #endregion

        #region Lifecycle Methods
        protected override void Start()
        {
            if (GameObject?.Scene == null)
                return;

            _cameraSystem = GameObject.Scene.GetSystem<CameraSystem>();
        }

        protected override void Update(float deltaTime)
        {
            MatchCameraRotation();
        }
        #endregion
    }
}
