using GDEngine.Core.Components;

namespace GDGame.Zones.Shared
{
    //marks a trigger volume as a portal to another scene
    //doesn't do switch itself - just carries the target scene name
    //the actual switching happens in one global listener in Main.cs that reacts whenever a trigger wit this component gets touched
    public sealed class ZonePortal : Component
    {
        private string _targetSceneName = string.Empty;

        //name of the scene to switch to
        //has to match whatever key was used when registering that scene with SceneManager
        public string TargetSceneName
        {
            get => _targetSceneName;
            set => _targetSceneName = value ?? string.Empty;
        }
    }
}
