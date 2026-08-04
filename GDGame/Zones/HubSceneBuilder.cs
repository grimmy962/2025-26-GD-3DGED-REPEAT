using GDEngine.Core.Entities;
using GDEngine.Core.Services;
using GDGame.Zones.Shared;
using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework;
using GDEngine.Core.Factories;

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
            BuildPortalToAudioZone(scene);
            BuildPortalToCameraZone(scene);

            return scene;
        }


        private static void BuildPortalToPhysicsZone(Scene scene, ZoneBuildContext buildContext)
        {
            var portalGO = new GameObject("Portal To PhysicsZone");
            portalGO.Transform.TranslateTo(new Vector3(10f, 1f, 10f));
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

        private static void BuildPortalToAudioZone(Scene scene)
        {
            var portalGO = new GameObject("Portal To AudioZone");
            portalGO.Transform.TranslateTo(new Vector3(-10f, 1f, 10f));

            var collider = portalGO.AddComponent<GDEngine.Core.Components.BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = portalGO.AddComponent<GDEngine.Core.Components.RigidBody>();
            rigidBody.BodyType = GDEngine.Core.Components.BodyType.Static;

            var portal = portalGO.AddComponent<ZonePortal>();
            portal.TargetSceneName = AudioZoneBuilder.SCENE_NAME;

            scene.Add(portalGO);
        }

        private static void BuildPortalToCameraZone(Scene scene)
        {
            var portalGO = new GameObject("Portal to CameraZone");
            portalGO.Transform.TranslateTo(new Vector3(0f, 1f, 15f));

            var collider = portalGO.AddComponent<GDEngine.Core.Components.BoxCollider>();
            collider.IsTrigger = true;

            var rigidBody = portalGO.AddComponent<GDEngine.Core.Components.RigidBody>();
            rigidBody.BodyType = GDEngine.Core.Components.BodyType.Static;

            var portal = portalGO.AddComponent<ZonePortal>();
            portal.TargetSceneName = CameraZoneBuilder.SCENE_NAME;

            scene.Add(portalGO);
        }
    }
}