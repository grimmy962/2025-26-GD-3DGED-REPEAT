using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Factories;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDGame.Zones.Shared;
using Microsoft.Xna.Framework;
using GDEngine.Core.Timing;

namespace GDGame.Zones
{
    //R2- Audio Zone
    //two spatial sound sources you can walk between to hear panning
    //a trigger that cycles through 3 background msuic tracks
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
            BuildSpatialSoundSource(scene, buildContext, "Gunshot Emitter", new Vector3(-8f, 0.5f, 10f), "hand_gun1", 1f);
            BuildSpatialSoundSource(scene, buildContext, "Laser Emitter", new Vector3(8f, 0.5f, 10f), "laser_gun_salve", 0.5f);
            BuildMusicSwitchTrigger(scene, buildContext);
            BuildReturnPortal(scene, buildContext);

			ZoneAnnotationFactory.Create(
				scene,
				buildContext.Graphics.GraphicsDevice,
				buildContext.Fonts.Get("perf_stats_font"),
				systemName: "Audio System",
				apiUsed: "AudioSystem, EventBus.Publish(PlayMusicEvent)",
				description: "Walk between the two sound sources to hear panning change.\n" +
							 "Walk into the glowing trigger to switch the music track.");

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

        //uses PeriodicSpatialSfxEmitter instead of a real loop (PlaySfxEvent has no loop option)
        //so it jsut keeps re-triggering itself evvery few seconds instead
		private static void BuildSpatialSoundSource(
			Scene scene, ZoneBuildContext buildContext, string name, Vector3 position, string soundKey, float volume)
		{
			var emitterGO = new GameObject(name);
            emitterGO.Layer = LayerMask.Interactables;
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
			emitter.Volume = volume;

			scene.Add(emitterGO);
		}

        //cycles through 3 real music tracks each time you touch it
        private static void BuildMusicSwitchTrigger(Scene scene, ZoneBuildContext buildContext)
        {
            var triggerGO = new GameObject("Music Switch Trigger");
            triggerGO.Transform.TranslateTo(new Vector3(0f, 1.5f, 15f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            triggerGO.AddComponent(meshFilter);
            var renderer = triggerGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("checkerboard");

            var collider = triggerGO.AddComponent<BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = triggerGO.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            scene.Add(triggerGO);

            float lastTriggeredTime = float.NegativeInfinity;
            const float COOLDOWN_SECONDS = 3f;

            string[] tracks = { "ambient_audio_zone", "ambient_audio_zone_2", "ambient_audio_zone_3" };
            int[] trackIndex = { 0 };

            //cooldown so this doesn't fire over and over while standing near the trigger
            //the engine reports touching every physics step, not just once on entry
            EngineContext.Instance.Events.Subscribe<GDEngine.Core.Events.TriggerEvent>(evt =>
            {
                if (evt.TriggerBody?.GameObject == triggerGO)
                {
                    if (Time.TimeSinceStartupSecs - lastTriggeredTime < COOLDOWN_SECONDS)
                    {
                        return;
                    }
                    lastTriggeredTime = Time.TimeSinceStartupSecs;

                    trackIndex[0] = (trackIndex[0] + 1) % tracks.Length;

                    EngineContext.Instance.Events.Publish(new PlayMusicEvent(tracks[trackIndex[0]], 0.15f, 1f));
                }
            });
        }

        private static void BuildReturnPortal(Scene scene, ZoneBuildContext buildContext)
        {
            var portalGO = new GameObject("Portal To Hub");
            portalGO.Transform.TranslateTo(new Vector3(0f, 1.5f, -5f));
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

        //called when the player walks in here from the hub
        public void StartAmbience()
        {
            EngineContext.Instance.Events.Publish(new PlayMusicEvent("ambient_audio_zone", 0.015f, 2f));
        }
    }
}
