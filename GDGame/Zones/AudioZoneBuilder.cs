using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Factories;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDGame.Demos.Controllers;
using GDGame.Zones.Shared;
using Microsoft.Xna.Framework;
using GDEngine.Core.Timing;

namespace GDGame.Zones
{
	public sealed class AudioZoneBuilder : IZoneBuilder
	{
		public const string SCENE_NAME = "AudioZone";
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
			ZonePlayerFactory.Create(scene, new Vector3(0f, 1.5f, 0f));
            BuildSpatialSoundSource(scene, buildContext, "Gunshot Emitter", new Vector3(-8f, 0.5f, 10f), "hand_gun1");
            BuildSpatialSoundSource(scene, buildContext, "Laser Emitter", new Vector3(8f, 0.5f, 10f), "laser_gun_salve");
            BuildMusicSwitchTrigger(scene);
			BuildReturnPortal(scene, buildContext);

			ZoneAnnotationFactory.Create(
				scene,
				buildContext.Graphics.GraphicsDevice,
				buildContext.Fonts.Get("perf_stats_font"),
				systemName: "Audio System",
				apiUsed: "AudioSystem, EventBus.Publish(PlayMusicEvent)",
				description: "Walk between the two sound sources to hear panning change.\n" +
							 "Walk into the glowing trigger to switch the music track.\n" +
							 "Left click any object to remove it (SFX via EventBus).");

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

		private static void BuildSpatialSoundSource(
			Scene scene, ZoneBuildContext buildContext, string name, Vector3 position, string soundKey)
		{
			var emitterGO = new GameObject(name);
			emitterGO.Transform.TranslateTo(position);
			emitterGO.Transform.ScaleTo(Vector3.One);

			var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
			emitterGO.AddComponent(meshFilter);
			var renderer = emitterGO.AddComponent<MeshRenderer>();
			renderer.Material = buildContext.MatBasicLit;
			renderer.Overrides.MainTexture = buildContext.Textures.Get("checkerboard");

			var emitter = emitterGO.AddComponent<PeriodicSpatialSfxEmitter>();
			emitter.ClipName = soundKey;
			emitter.IntervalSeconds = 2.5f;
			emitter.Volume = 1f;

			scene.Add(emitterGO);
		}

		private static void BuildMusicSwitchTrigger(Scene scene)
		{
			var triggerGO = new GameObject("Music Switch Trigger");
			triggerGO.Transform.TranslateTo(new Vector3(0f, 1f, 15f));

			var collider = triggerGO.AddComponent<BoxCollider>();
			collider.Size = new Vector3(2f, 3f, 2f);
			collider.IsTrigger = true;

			var rigidBody = triggerGO.AddComponent<RigidBody>();
			rigidBody.BodyType = BodyType.Static;

			scene.Add(triggerGO);

			float lastTriggeredTime = float.NegativeInfinity;
			const float COOLDOWN_SECONDS = 3f;

            EngineContext.Instance.Events.Subscribe<GDEngine.Core.Events.TriggerEvent>(evt =>
            {
                if (evt.TriggerBody?.GameObject == triggerGO)
                {
                    if (Time.TimeSinceStartupSecs - lastTriggeredTime < COOLDOWN_SECONDS)
                    {
						return;
					}
					lastTriggeredTime = Time.TimeSinceStartupSecs;

                    EngineContext.Instance.Events.Publish(new StopMusicEvent(1f));
                    EngineContext.Instance.Events.Publish(new PlayMusicEvent("explosion1", 0.7f, 1f));
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

        public void StartAmbience()
        {
            EngineContext.Instance.Events.Publish(new PlayMusicEvent("ambient_audio_zone", 0.015f, 2f));
        }
    }
}
