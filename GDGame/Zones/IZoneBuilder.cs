
using GDEngine.Core.Entities;
using GDGame.Zones.Shared;

namespace GDGame.Zones
{
    // contract implemented by every hub/zone builder in the ca showcase
    // each implementation is responsible for exactly one Scene
    // it wires up that scene's systems, populates ts GameObjects, and returns the finished ready to run scene
    public interface IZoneBuilder
    {
        //unique key this scene is registered udner via SceneManager.AddScene and later activate with SceneManager.SetActiveScene
        string SceneName
        { 
            get;
        }

        //build and returns a fully populated scene: core systems, demosntration content and any zone transition portals
        //buildContext = shared references (content, graphics, scene manager) common to every zone
        //returns the finished scene
        Scene Build(ZoneBuildContext buildContext);
    }
}
