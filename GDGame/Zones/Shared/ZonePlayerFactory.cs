using GDEngine.Core.Components;
using GDEngine.Core.Components.Controllers.Physics;
using GDEngine.Core.Entities;
using GDEngine.Core.Rendering.Base;
using Microsoft.Xna.Framework;

namespace GDGame.Zones.Shared
{
    //builds the first-person capsule (movement/physics) plus its child camera (mouse look only)
    //every zone calls this the same way
    //so i pulled it out here instead of copying it into every zone builder
    public static class ZonePlayerFactory
    {
        #region Constants

        private const float DEFAULT_MOVE_SPEED = 8.0F;
        private const float DEFAULT_ACCELERATION = 50.0F;
        private const float DEFAULT_GROUND_FRICTION = 10.0F;
        private const float DEFAULT_CAPSULE_RADIUS = 0.5f;
        private const float DEFAULT_CAPSULE_HEIGHT = 1.8f;
        private const float DEFAULT_GROUND_CHECK_DISTANCE = 0.25f;

        #endregion

        #region Methods
        //returns the capsule GameObject so callers can attach extra stuff to it (like a visible body model in the Camera zone)
        public static GameObject Create(Scene scene, Vector3 spawnPosition)
        {
            var parentGO = new GameObject(AppData.CAMERA_NAME_FIRST_PERSON_PARENT);
            parentGO.Layer = LayerMask.IgnoreRaycast;
            parentGO.Transform.TranslateTo(spawnPosition);

            var fpsController = parentGO.AddComponent<FirstPersonCapsuleController>();
            fpsController.MoveSpeed = DEFAULT_MOVE_SPEED;
            fpsController.Acceleration = DEFAULT_ACCELERATION;
            fpsController.GroundFriction = DEFAULT_GROUND_FRICTION;
            fpsController.CapsuleRadius = DEFAULT_CAPSULE_RADIUS;
            fpsController.CapsuleHeight = DEFAULT_CAPSULE_HEIGHT;
            fpsController.GroundCheckDistance = DEFAULT_GROUND_CHECK_DISTANCE;

            //camera is a child so mouse look only rotates the view, not the capsule itself
            //the capsule's own facing is usesd for WASD movement direction instead (see FirstPersonCapsuleController)
            var cameraGO = new GameObject(AppData.CAMERA_NAME_FIRST_PERSON);
            cameraGO.Transform.SetParent(parentGO.Transform);
            cameraGO.Transform.TranslateTo(Vector3.Zero);

            var camera = cameraGO.AddComponent<Camera>();
            camera.FieldOfView = MathHelper.ToRadians(80f);
            cameraGO.AddComponent<MouseYawPitchController>();

            scene.Add(parentGO);
            scene.Add(cameraGO);

            scene.SetActiveCamera(AppData.CAMERA_NAME_FIRST_PERSON);

            return parentGO;
        }

        #endregion
    }
}
