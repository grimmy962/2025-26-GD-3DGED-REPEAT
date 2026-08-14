using GDEngine.Core.Entities;
using GDEngine.Core.Services;
using GDGame.Zones.Shared;
using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework;
using GDEngine.Core.Factories;
using GDEngine.Core.Audio;

namespace GDGame.Zones
{
    //the hub is the central space you spawn into and return to between zones
    //just five portals arranged in a citcle, a floating label above each one, and some ambient sound
    public sealed class HubSceneBuilder : IZoneBuilder
    {
        public const string SCENE_NAME = "Hub";
        public string SceneName => SCENE_NAME;

        public Scene Build(ZoneBuildContext buildContext)
        {
            var scene = new Scene(buildContext.EngineContext);

            ZoneSystemFactory.AddCoreSystems(
                scene,
                buildContext.Graphics,
                buildContext.Sounds,
                includePhysics: true,
                gravity: AppData.GRAVITY);

            BuildPortalToPhysicsZone(scene, buildContext);
            BuildPortalToAudioZone(scene, buildContext);
            BuildPortalToCameraZone(scene, buildContext);
            BuildPortalToOrchestrationZone(scene, buildContext);
            BuildPortalToEventsStateZone(scene, buildContext);
            BuildAmbientEmitter(scene, buildContext);

            //floating name above each portal so you know where it goes before walking in
            WorldSpaceLabelFactory.Create(scene, buildContext.Graphics.GraphicsDevice, buildContext.Fonts.Get("perf_stats_font"), new Vector3(0f, 3f, 20f), "Physics Zone");
            WorldSpaceLabelFactory.Create(scene, buildContext.Graphics.GraphicsDevice, buildContext.Fonts.Get("perf_stats_font"), new Vector3(19f, 3f, 6f), "Audio Zone");
            WorldSpaceLabelFactory.Create(scene, buildContext.Graphics.GraphicsDevice, buildContext.Fonts.Get("perf_stats_font"), new Vector3(12f, 3f, -16f), "Camera Zone");
            WorldSpaceLabelFactory.Create(scene, buildContext.Graphics.GraphicsDevice, buildContext.Fonts.Get("perf_stats_font"), new Vector3(-12f, 3f, -16f), "Orchestration Zone");
            WorldSpaceLabelFactory.Create(scene, buildContext.Graphics.GraphicsDevice, buildContext.Fonts.Get("perf_stats_font"), new Vector3(-19f, 3f, 6f), "Events & State Zone");

            ZoneAnnotationFactory.Create(
                scene,
                buildContext.Graphics.GraphicsDevice,
                buildContext.Fonts.Get("perf_stats_font"),
                systemName: "SceneManager & EventBus (Observer)",
                apiUsed: "SceneManager.SetActiveScene(...), EventBus.Subscribe<TriggerEvent>(...)",
                description: "Walk into any glowing portal to travel to that zone.\n" +
                             "Each portal publishes a TriggerEvent; a single global\n" +
                             "listener reacts by switching the active scene.");

            return scene;
        }


        private static void BuildPortalToPhysicsZone(Scene scene, ZoneBuildContext buildContext)
        {
            var portalGO = new GameObject("Portal To PhysicsZone");
            portalGO.Transform.TranslateTo(new Vector3(0f, 1.5f, 20f));
            portalGO.Transform.ScaleTo(new Vector3(2f, 3f, 0.2f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            portalGO.AddComponent(meshFilter);
            var renderer = portalGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");

            var collider = portalGO.AddComponent<GDEngine.Core.Components.BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = portalGO.AddComponent<GDEngine.Core.Components.RigidBody>();
            rigidBody.BodyType = GDEngine.Core.Components.BodyType.Static;

            var portal = portalGO.AddComponent<ZonePortal>();
            portal.TargetSceneName = PhysicsZoneBuilder.SCENE_NAME;

            scene.Add(portalGO);
        }

        private static void BuildPortalToAudioZone(Scene scene, ZoneBuildContext buildContext)
        {
            var portalGO = new GameObject("Portal To AudioZone");
            portalGO.Transform.TranslateTo(new Vector3(19f, 1.5f, 6f));
            portalGO.Transform.ScaleTo(new Vector3(2f, 3f, 0.2f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            portalGO.AddComponent(meshFilter);
            var renderer = portalGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");


            var collider = portalGO.AddComponent<GDEngine.Core.Components.BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = portalGO.AddComponent<GDEngine.Core.Components.RigidBody>();
            rigidBody.BodyType = GDEngine.Core.Components.BodyType.Static;

            var portal = portalGO.AddComponent<ZonePortal>();
            portal.TargetSceneName = AudioZoneBuilder.SCENE_NAME;

            scene.Add(portalGO);
        }

        private static void BuildPortalToCameraZone(Scene scene, ZoneBuildContext buildContext)
        {
            var portalGO = new GameObject("Portal to CameraZone");
            portalGO.Transform.TranslateTo(new Vector3(12f, 1.5f, -16f));
            portalGO.Transform.ScaleTo(new Vector3(2f, 3f, 0.2f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            portalGO.AddComponent(meshFilter);
            var renderer = portalGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");

            var collider = portalGO.AddComponent<GDEngine.Core.Components.BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = portalGO.AddComponent<GDEngine.Core.Components.RigidBody>();
            rigidBody.BodyType = GDEngine.Core.Components.BodyType.Static;

            var portal = portalGO.AddComponent<ZonePortal>();
            portal.TargetSceneName = CameraZoneBuilder.SCENE_NAME;

            scene.Add(portalGO);
        }

        private static void BuildPortalToOrchestrationZone(Scene scene, ZoneBuildContext buildContext)
        {
            var portalGO = new GameObject("Portal to OrchestrationZone");
            portalGO.Transform.TranslateTo(new Vector3(-12f, 1.5f, -16f));
            portalGO.Transform.ScaleTo(new Vector3(2f, 3f, 0.2f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            portalGO.AddComponent(meshFilter);
            var renderer = portalGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");

            var collider = portalGO.AddComponent<GDEngine.Core.Components.BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = portalGO.AddComponent<GDEngine.Core.Components.RigidBody>();
            rigidBody.BodyType = GDEngine.Core.Components.BodyType.Static;

            var portal = portalGO.AddComponent<ZonePortal>();
            portal.TargetSceneName = OrchestrationZoneBuilder.SCENE_NAME;

            scene.Add(portalGO);
        }

        private static void BuildPortalToEventsStateZone(Scene scene, ZoneBuildContext buildContext)
        {
            var portalGO = new GameObject("Portal to EventsStateZone");
            portalGO.Transform.TranslateTo(new Vector3(-19f, 1.5f, 6f));
            portalGO.Transform.ScaleTo(new Vector3(2f, 3f, 0.2f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            portalGO.AddComponent(meshFilter);
            var renderer = portalGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");

            var collider = portalGO.AddComponent<GDEngine.Core.Components.BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = portalGO.AddComponent<GDEngine.Core.Components.RigidBody>();
            rigidBody.BodyType = GDEngine.Core.Components.BodyType.Static;

            var portal = portalGO.AddComponent<ZonePortal>();
            portal.TargetSceneName = EventsStateZoneBuilder.SCENE_NAME;

            scene.Add(portalGO);
        }

        //quiet looping-ish ambient sound in the hub
        //using an sfx emitter instead of PlayMusicEvent on purpose
        //every zone's AudioSystem listens to the same global music chanel, so real musi here
        //would keep playing even after elaving the Hub
        //this way it can't 'bleed' into other zones
        private static void BuildAmbientEmitter(Scene scene, ZoneBuildContext buildContext)
        {
            var emitterGO = new GameObject("Hub Ambient Emitter");
            emitterGO.Transform.TranslateTo(new Vector3(0f, 3f, 0f));

            var emitter = emitterGO.AddComponent<PeriodicSpatialSfxEmitter>();
            emitter.ClipName = "calm_loop";
            emitter.IntervalSeconds = 6f;
            emitter.Volume = 0.15f;

            scene.Add(emitterGO);
        }
    }
}