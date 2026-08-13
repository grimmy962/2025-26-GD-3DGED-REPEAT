using GDEngine.Core.Entities;
using GDEngine.Core.Services;
using GDGame.Zones.Shared;
using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework;
using GDEngine.Core.Factories;
using GDGame.Zones.Shared;

namespace GDGame.Zones
{
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

            WorldSpaceLabelFactory.Create(scene, buildContext.Graphics.GraphicsDevice, buildContext.Fonts.Get("perf_stats_font"), new Vector3(0f, 3f, 20f), "Physics Zone");
            WorldSpaceLabelFactory.Create(scene, buildContext.Graphics.GraphicsDevice, buildContext.Fonts.Get("perf_stats_font"), new Vector3(19f, 3f, 6f), "Audio Zone");
            WorldSpaceLabelFactory.Create(scene, buildContext.Graphics.GraphicsDevice, buildContext.Fonts.Get("perf_stats_font"), new Vector3(12f, 3f, -16f), "Camera Zone");
            WorldSpaceLabelFactory.Create(scene, buildContext.Graphics.GraphicsDevice, buildContext.Fonts.Get("perf_stats_font"), new Vector3(-12f, 3f, -16f), "Orchestration Zone");
            WorldSpaceLabelFactory.Create(scene, buildContext.Graphics.GraphicsDevice, buildContext.Fonts.Get("perf_stats_font"), new Vector3(-19f, 3f, 6f), "Events & State Zone");

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
    }
}