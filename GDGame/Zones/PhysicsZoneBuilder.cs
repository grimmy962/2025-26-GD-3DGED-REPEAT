using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using GDGame.Demos.Controllers;
using GDGame.Zones.Shared;
using Microsoft.Xna.Framework;
using GDEngine.Core.Factories;

namespace GDGame.Zones
{
	public sealed class PhysicsZoneBuilder : IZoneBuilder
	{
		public const string SCENE_NAME = "PhysicsZone";
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

			BuildGround(scene, buildContext);
			ZonePlayerFactory.Create(scene, new Vector3(0f, 5f, 0f));
			BuildCrates(scene, buildContext);
			BuildObservableTrigger(scene);
			BuildRaycastInteractor(scene);
            BuildReturnPortal(scene, buildContext);

            ZoneAnnotationFactory.Create(
				scene,
				buildContext.Graphics.GraphicsDevice,
				buildContext.Fonts.Get("perf_stats_font"),
				systemName: "Physics System",
				apiUsed: "RigidBody, BoxCollider, PhysicsSystem.RaycastFromScreen(...)",
				description: "Crates fall and collide with each other and the floor." +
							 "Look at a crate and left click to remove it (raycast)." +
							 "Walk into the glowing trigger to exit the scene (you will hear a sound).");
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

		private static void BuildCrates(Scene scene, ZoneBuildContext buildContext)
		{
			Vector3[] positions =
			{
				new Vector3(-3f, 6f, 10f),
				new Vector3(0f, 8f, 10f),
				new Vector3(3f, 10f, 10f),
			};

			foreach (var position in positions)
			{
				var crate = new GameObject("crate");
                crate.Layer = LayerMask.Interactables;
                crate.Transform.TranslateTo(position);
				crate.Transform.ScaleTo(Vector3.One);

                var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
                crate.AddComponent(meshFilter);

                var renderer = crate.AddComponent<MeshRenderer>();
                renderer.Material = buildContext.MatBasicLit;
                renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");

				var collider = crate.AddComponent<BoxCollider>();
				collider.Size = Vector3.One;

				var rigidBody = crate.AddComponent<RigidBody>();
				rigidBody.BodyType = BodyType.Dynamic;
				rigidBody.Mass = 1.0f;

				scene.Add(crate);
			}
		}

		private static void BuildObservableTrigger(Scene scene)
		{
			var triggerGO = new GameObject("Physics Observable Trigger");
			triggerGO.Transform.TranslateTo(new Vector3(6f, 1f, 5f));

			var collider = triggerGO.AddComponent<BoxCollider>();
			collider.Size = new Vector3(2f, 3f, 2f);
			collider.IsTrigger = true;

			var rigidBody = triggerGO.AddComponent<RigidBody>();
			rigidBody.BodyType = BodyType.Static;

			scene.Add(triggerGO);

			EngineContext.Instance.Events.Subscribe<GDEngine.Core.Events.TriggerEvent>(evt =>
			{
				if (evt.TriggerBody?.GameObject == triggerGO)
				{
					EngineContext.Instance.Events.Publish(
						new GDEngine.Core.Audio.PlaySfxEvent("SFX_UI_Click_Designed_Pop_Generic_1", 1f, false, null));
				}
			});
		}

		private static void BuildRaycastInteractor(Scene scene)
		{
			var interactorGO = new GameObject("Interactor");
			var interaction = interactorGO.AddComponent<InteractionComponent>();
			interaction.MaxDistance = 50f;
            interaction.HitMask = LayerMask.Interactables;
            scene.Add(interactorGO);
		}

		private static void BuildReturnPortal(Scene scene, ZoneBuildContext buildContext)
		{
			var portalGO = new GameObject("Portal To Hub");
			portalGO.Transform.TranslateTo(new Vector3(0f, 1f, -5f));
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
