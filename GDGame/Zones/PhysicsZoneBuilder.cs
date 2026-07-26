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
using System.Collections.Generic;

namespace GDGame.Zones
{
	public sealed class PhysicsZoneBuilder : IZoneBuilder
	{
		public const string SCENE_NAME = "PhysicsZone";
		private const int GROUND_SCALE = 250;
		public string SceneName => SCENE_NAME;
		private Scene? _scene;
		private ZoneBuildContext? _buildContext;
		private readonly List<GameObject> _crates = new();

		public Scene Build(ZoneBuildContext buildContext)
		{
			var scene = new Scene(buildContext.EngineContext, SCENE_NAME);
			_scene = scene;
			_buildContext = buildContext;

			ZoneSystemFactory.AddCoreSystems(
				scene,
				buildContext.Graphics,
				buildContext.Sounds,
				includePhysics: true,
				gravity: AppData.GRAVITY);

			BuildGround(scene, buildContext);
            ZonePlayerFactory.Create(scene, new Vector3(0f, 1.5f, 0f)); BuildCrates(scene, buildContext);
			BuildObservableTrigger(scene);
			BuildRaycastInteractor(scene);
            BuildReturnPortal(scene, buildContext);

            ZoneAnnotationFactory.Create(
				scene,
				buildContext.Graphics.GraphicsDevice,
				buildContext.Fonts.Get("perf_stats_font"),
				systemName: "Physics System",
				apiUsed: "RigidBody, BoxCollider, PhysicsSystem.RaycastFromScreen(...)",
				description: "Monkeys fall and collide with each other and the floor.\n" +
							 "Look at a monkey and left click to remove it (raycast).\n" +
							 "Walk into the teleport to exit the scene (you will hear a sound).");
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

		private void BuildCrates(Scene scene, ZoneBuildContext buildContext)
		{
			Vector3[] positions =
			{
                new Vector3(0f, 6f, 10f),
				new Vector3(0.3f, 7f, 10.2f),
				new Vector3(-0.2f, 8f, 9.8f),
            };

			var model = buildContext.Models.Get("monkey1");

			foreach (var position in positions)
			{
				var fallingObject = new GameObject("falling object");
                fallingObject.Layer = LayerMask.Interactables;
                fallingObject.Transform.TranslateTo(position);
				fallingObject.Transform.ScaleTo(Vector3.One);

                var meshFilter = MeshFilterFactory.CreateFromModel(model, buildContext.Graphics.GraphicsDevice, 0, 0);
                fallingObject.AddComponent(meshFilter);

                var renderer = fallingObject.AddComponent<MeshRenderer>();
                renderer.Material = buildContext.MatBasicLit;
                renderer.Overrides.MainTexture = buildContext.Textures.Get("mona lisa");

                var collider = fallingObject.AddComponent<SphereCollider>();
                collider.Diameter = 1.0f;

                var rigidBody = fallingObject.AddComponent<RigidBody>();
				rigidBody.BodyType = BodyType.Dynamic;
				rigidBody.Mass = 1.0f;

				scene.Add(fallingObject);
                _crates.Add(fallingObject);
            }
		}

		public void ResetCrates()
		{
			if(_scene == null || _buildContext == null)
			{
				return;
			}

			foreach (var crate in _crates)
			{
				_scene.Remove(crate);
			}

			_crates.Clear();
			BuildCrates(_scene, _buildContext);
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
