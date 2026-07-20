using System;


namespace GDGame.Zones.Shared
{
	public static class ZonePlayerFactory
	{
		private const float DEFAULT_MOVE_SPEED = 8.0F;
		private const float DEFAULT_ACCELERATION = 50.0F;
		private const float DEFAULT_GROUND_FRICTION = 10.0F;
		private const float DEFAULT_JUMP_IMPULSE = 7.0f;
		private const float DEFAULT_CAPSULE_RADIUS = 0.05f;
		private const float DEFAULT_CAPSULE_HEIGHT = 1.8f;
		private const float DEFAULT_GROUND_CHECK_DISTANCE = 0.25f;

		public static void Create(Scene scene, Vector3 spawnPosition)
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

			var cameraGO = new GameObject(AppData.CAMERA_NAME_FIRST_PERSON);
			cameraGO.Transform.SetParent(parentGO.Transform);
			cameraGO.Transform.TranslateTo(Vector3.Zero);

			var camera = cameraGO.AddComponent<Camera>();
			camera.FieldOfView = MathHelper.ToRadians(80f);
			cameraGO.AddComponent<MouseYawPitchController>();

			scene.Add(parentGO);
			scene.Add(cameraGO);

			scene.SetActiveCamera(AppData.CAMERA_NAME_FIRST_PERSON);
		}
	}
}
