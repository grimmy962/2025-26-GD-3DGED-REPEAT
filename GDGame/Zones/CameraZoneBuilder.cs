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
	public sealed class CameraZoneBuilder : IZoneBuilder
	{
		public const string SCENE_NAME = "CameraZone";
		private const int GROUND_SCALE = 100;
		private const string CAMERA_FIRST_PERSON = "CamZone First Person";
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
				description: "Walk into a camera mode:\n"+
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
			collider.Center = new Vector3(0, 0, -0.0125f);

			var rigidBody = ground.AddComponent<RigidBody>();
			rigidBody.BodyType = BodyType.Static;
			ground.IsStatic = true;
			ground.Layer = LayerMask.Ground;

			scene.Add(ground);
		}

		private static void BuildFirstPersonCamera(Scene scene)
		{
			ZonePlayerFactory.Create(scene, new Vector3(0f, 1.5f, 0f));
		}

        private static void BuildThirdPersonCamera(Scene scene)
        {
            var cameraGO = new GameObject(CAMERA_THIRD_PERSON);
            cameraGO.AddComponent<Camera>();

            var thirdPersonController = new ThirdPersonController();
            thirdPersonController.TargetName = AppData.CAMERA_NAME_FIRST_PERSON_PARENT;
            thirdPersonController.ShoulderOffset = 0;
            thirdPersonController.FollowDistance = 10;
            thirdPersonController.RotationDamping = 20;
            cameraGO.AddComponent(thirdPersonController);

            scene.Add(cameraGO);
        }
		
		private static void BuildCinematicCamera (Scene scene)
		{
			var cameraGO = new GameObject(CAMERA_CINEMATIC);
			cameraGO.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(-90), 0, 0), true);
			cameraGO.AddComponent<Camera>();

			var curveController = cameraGO.AddComponent<CurveController>();
			curveController.PositionCurve = BuildCinematicPositionCurve();
			curveController.TargetCurve = BuildCinematicTargetCurve();
			curveController.Duration = 10;

			scene.Add(cameraGO);
		}

        private static AnimationCurve3D BuildCinematicPositionCurve()
        {
            var curve = new AnimationCurve3D(CurveLoopType.Oscillate);
            curve.AddKey(new Vector3(-15, 12, 5), 0f);
            curve.AddKey(new Vector3(0, 12, -5), 0.5f);
            curve.AddKey(new Vector3(15, 12, 5), 1f);
            return curve;
        }

        private static AnimationCurve3D BuildCinematicTargetCurve()
		{
			var curve = new AnimationCurve3D(CurveLoopType.Constant);
			curve.AddKey(new Vector3(0, 0, 10), 0f);
			curve.AddKey(new Vector3(0, 0, 10), 1f);
			return curve;
		}

		private static void BuildCameraSwitchTrigger(Scene scene, Vector3 position, string name, string targetCameraName)
		{
			var triggerGO = new GameObject(name);
			triggerGO.Transform.TranslateTo(position);

			var collider = triggerGO.AddComponent<BoxCollider>();
			collider.Size = new Vector3(2f, 3f, 2f);
			collider.IsTrigger = true;

			var rigidBody = triggerGO.AddComponent<RigidBody>();
			rigidBody.BodyType = BodyType.Static;

			scene.Add(triggerGO);

			float lastTriggeredTime = float.NegativeInfinity;
			const float COOLDOWN_SECONDS = 1.5f;

			EngineContext.Instance.Events.Subscribe<GDEngine.Core.Events.TriggerEvent>(evt =>
			{
				if (evt.TriggerBody?.GameObject == triggerGO)
				{
					if (Time.TimeSinceStartupSecs - lastTriggeredTime < COOLDOWN_SECONDS)
					{
						return;
					}
					lastTriggeredTime = Time.TimeSinceStartupSecs;

					scene.SetActiveCamera(targetCameraName);
				}
			});
		}

        private static void BuildReturnPortal(Scene scene, ZoneBuildContext buildContext)
        {
            var portalGO = new GameObject("Portal To Hub");
            portalGO.Transform.TranslateTo(new Vector3(0f, 1f, 20f));
            portalGO.Transform.ScaleTo(new Vector3(2f, 3f, 0.2f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            portalGO.AddComponent(meshFilter);
            var renderer = portalGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");

            var collider = portalGO.AddComponent<BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = portalGO.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            var portal = portalGO.AddComponent<ZonePortal>();
            portal.TargetSceneName = HubSceneBuilder.SCENE_NAME;

            scene.Add(portalGO);
        }
    }
}
