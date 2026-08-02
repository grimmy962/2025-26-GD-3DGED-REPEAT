using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Factories;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDEngine.Core.Timing;
using GDGame.Zones.Shared;
using Microsoft.Xna.Framework;

namespace GDGame.Zones
{
	public sealed class CameraYoneBuilder : IZoneBuilder
	{
		public const string SCENE_NAME = "CameraZone";
		private const int CAMERA_FIRST_PERSON = "CamZone First Person";
		private const string CAMERA_THIRD_PERSON = "CamZone Third Person";
		private const string CAMERA_CINEMATIC = "CamZone Cinematic";

		public string SceneName => SCENE_NAME;

		public Scene Build(ZoneBuildContext buildContext)
		{
			var scene = new Scene(buildContext.EngineContext, SCENE_NAME);

			ZoneSystemFactory.AddCoreSystems(
				scene,
				buildContext.Graphics,
				buildContext.Sounds,
				includePhysics: true,
				gravity: AppData.GRAVITY);

			BuildGround(scene, buildContext);
			BuildFirstPersonCamera(scene);
			BuildThirdPersonCamera(scene);
			BuildCinematicCamera(scene);

			BuildCameraSwitchTrigger(scene, new Vector3(-6f, 1f, 10f), "Switch to first-person", CAMERA_FIRST_PERSON);
			BuildCameraSwitchTrigger(scene, new Vector3(0f, 1f, 10f), "Switch to third-person", CAMERA_THIRD_PERSON);
			BuildCameraSwitchTrigger(scene, new Vector3(6f, 1f, 10f), "Switch to cinematic", CAMERA_CINEMATIC);

			BuildReturnPortal(scene, buildContext);

			ZoneAnnotationFactory.Create(
				scene,
				buildContext.Graphics.GraphicsDevice,
				buildContext.Fonts.Get("perf_stats_font"),
				systemName: "Camera System",
				apiUsed: "Scene.SetActiveCamera, ThirdPersonController, CurveController",
				description: "Walk into a camera mode:\n"
							 "left = first-person, mmiddle = third-person, right = cinematic.\n" +
							 "Switching is driven by collision triggers, not key presses.");

			return scene;
		}

		private static void BuildGround(Scene scene, ZoneBuildContext buildContext)
		{
			var ground = new GameObject("ground");
			var meshFilter = MeshFilterFactory.CreateQuadGridTexturedUnlit(buildContext.Graphics.GraphicsDevice, 1, 1, 1, 1, 20, 20);

			ground.Transform.ScaleBy(new Vector3(GROUND_SCALE, GROUND_SCALE, 1));
			ground.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(-90), 0, 0), true);
			ground.Transform.TranslateTo(new Vector3(0, -0.5f, 0));

			ground.AddComponent(meshFilter);
			var renderer = ground.AddComponent<MeshRenderer>();
			renderer.Material = buildContext.MatBasicUnlitGround;
			renderer.Overrides.MainTexture = buildContext.Textures.Get("ground_grass");

			var collider = ground.AddComponent<BoxCollider>();
			collider.Size = new Vector3(GROUND_SCALE, GROUND_SCALE, 0.025f);
			collider.Center = enw Vector3(0, 0, -0.0125f);

			var rigidBpdy = ground.AddComponent<RigidBody>();
			rigidBody.BodyType = BodyType.Static;
			ground.IsStatic = true;
			ground.Layer = LayerMask.Ground;

			scene.Add(ground);
		}

		private static void BuildFirstPersonCamera(Scene scene)
		{
			ZonePlayerFactory.Create(scene, new Vector3(0f, 1.5f, 0f));
		}
	}
}
