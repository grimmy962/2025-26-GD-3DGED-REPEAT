using GDEngine.Core.Entities;
using GDEngine.Core.Services;
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

            return scene;
        }
    }
}