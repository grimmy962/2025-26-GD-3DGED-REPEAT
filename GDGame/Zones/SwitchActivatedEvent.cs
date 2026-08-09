using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Events;
using GDEngine.Core.Factories;
using GDEngine.Core.Gameplay;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using GDEngine.Core.Timing;
using GDGame.Zones.Shared;
using Microsoft.Xna.Framework;

namespace GDGame.Zones
{
	public sealed class SwitchActivatedEvent
	{
		public string SwitchName{
			get;
		}
		public SwitchActivatedEvent(string switchName){
			SwitchName = switchName;
		}
	}

	public sealed class AlarmTriggeredEvent
	{ 
		public string ZoneName { 
			get; 
		}

		public AlarmTriggeredEvent(string zoneName) {
			ZoneName = zoneName; 
		}
	}

	public sealed class EventsStateZoneBuilder : IZoneBuilder
	{
		public const string SCENE_NAME = "EventsStateZone";
		private const int GROUND_SCALE = 100;

		public string SceneName => SCENE_NAME;

		private GameStateSystem? _gameStateSystem;
		private bool[]? _switchActivated;
		private string[]? _statusText;

		public Scene Build(ZoneBuildContext buildContext)
		{
			var scene = new Scene(buildContext.EngineContext, SCENE_NAME);

			ZoneSystemFactory.AddCoreSystems(
				scene,
				buildContext.Graphics,
				buildContext.Sounds,
				includePhysics: true,
				gravity: AppData.GRAVITY);

			var gameStateSystem = scene.AddSystem(new GameStateSystem());
			_gameStateSystem = gameStateSystem;

			BuildGround(scene, buildContext);
			ZonePlayerFactory.Create(scene, new Vector3(0f, 1.5f, 0f));

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
	}
}
