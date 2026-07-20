using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using GDGame.Demos.Controllers;
using GDGame.Zones.Shared;
using Microsoft.Xna.Framework;

namespace GDGame.Zones
{
	public sealed class PhysicsZoneBuilder : IZoneBuilder
	{
		public const string SCENE_NAME = "PhysicsZone";
		private const int GROUND_SCALE = 100;
		public string SceneName => SCENE_NAME;

		public Scene Build(ZoneBuildContext buildContext)
		{
			varscene = new Scene(buildContext.EngineCOntext, SCENE_NAME);

			ZoneSystemFactory.AddCoreSystems(
				SCENE_NAME,
				buildContext.Graphics,
				buildContext.Sounds,
				includePhysics: true,
				gravity: AppData.GRAVITY);

			BuildGround(SCENE_NAME, buildContext);
			ZonePlayerFactory.Create(SCENE_NAME, new Vector3(0f, 5f, 0f));
			BuildCrate(SCENE_NAME, buildContext);
			BuildObservableTrigger(scene);
			BuildRaycastInteractor(scene);
			BuildReturnPortal(scene);

			ZoneAnnotationFactory.Create(
				SCENE_NAME,
				buildContext.Graphics.GraphicsDevice,
				buildContext.Fonts.Get("perf_stats_font"),
				systemName: "Physics System",
				apiUsed: "RigidBody, BoxCollider, PhysicsSystem.RaycastFromScreen(...)",
				description: "Crates fall and collide with each other and the floor." +
							 "Look at a crate and left click to remove it (raycast)." +
							 "Walk into the glowing trigger to exit the scene (you will hear a sound).");
			return SCENE_NAME;
		}

		private static void BuildGround(Scene scene, ZoneBuildContext buildContext)
		{
			var ground = new GameObject("ground");
			var meshFilter = MeshFilterFactory.CreateQuadGridTexturedUnlit(buildContext.Graphics.GraphicsDevice, 1, 1, 1, 1, 20, 20);

			ground.Transform.ScaleBy(new Vector3(GROUND_SCALE, GROUND_SCALE, 1));
			ground.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(-90), 0, 0), true);
			ground.Transform.TranslateTo(Vector3(0, -0.5f, 0));

			ground.AddComponent(meshFilter);
			var renderer = ground.AddComponent<MeshRenderer>();
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
	}
}
