using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Events;
using GDEngine.Core.Factories;
using GDEngine.Core.Orchestration;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using GDEngine.Core.Timing;
using GDGame.Zones.Shared;
using Microsoft.Xna.Framework;

namespace GDGame.Zones
{
	public sealed class OrchestrationZoneBuilder : IZoneBuilder
	{
		public const string SCENE_NAME = "OrchestrationZone";
		private const string SEQUENCE_NAME = "artifact_ritual";
		private const float ARTIFACT_RISE_TARGET_Y = 3.5f;
        private const int GROUND_SCALE = 100;

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

			var orchestrationSystem = new OrchestrationSystem();
			orchestrationSystem.Configure(options =>
			{
				options.Time = Orchestrator.OrchestrationTime.Unscaled;
				options.LocalScale = 1;
				options.Paused = false;
			});

			scene.Add(orchestrationSystem);

			BuildGround(scene, buildContext);
			ZonePlayerFactory.Create(scene, new Vector3(0f, 1.5f, 0f));

			var artifactGO = BuildRitualArtifact(scene, buildContext);
		//	var statusTextProvider = BuildStatusText(scene, buildContext);

		//	RegisterRitualSequence(orchestrationSystem, artifactGO, statusTextProvider);
		//	BuildTrigger(scene, orchestrationSystem);

		//	BuildReturnPortal(scene, buildContext);

		//	ZoneAnnotationFactory.Create(
		//		scene,
		//		buildContext.Graphics.GraphicsDevice,
		//		buildContext.Fonts.Get("perf_stats_font"),
		//		systemName: "Orchestration System",
		//		apiUsed: "OrchestrationSystem, Orchestrator.Builder (WaitSeconds, Publish, Do, If)",
		//		description: "Walk into the glowing trigger to start the ritual sequence.\n"+
		//					 "8 steps span UI, Audio, and Transform, including a conditional\n"+
		//					 "step that changes the outcome message.")
			return scene;
		}

        private static void BuildGround(Scene scene, ZoneBuildContext buildContext)
        {
            var ground = new GameObject("ground");
            var meshFilter = MeshFilterFactory.CreateQuadGridTexturedUnlit(
                buildContext.Graphics.GraphicsDevice, 1, 1, 1, 1, 20, 20);

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

		private static GameObject BuildRitualArtifact(Scene scene, ZoneBuildContext buildContext)
		{
			var artifactGO = new GameObject("Ritual Artifact");
			artifactGO.Transform.TranslateTo(new Vector3(0f, 0.5f, 10f));

			var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
			artifactGO.AddComponent(meshFilter);

			var renderer = artifactGO.AddComponent<MeshRenderer>();
			renderer.Material = buildContext.MatBasicLit;
			renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");

			scene.Add(artifactGO);
			return artifactGO;
		}
    }
}
