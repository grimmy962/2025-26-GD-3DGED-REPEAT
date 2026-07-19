using GDEngine.Core.Components;

namespace GDGame.Zones.Shared
{
    public sealed class ZonePortal : Component
    {
        private string _targetSceneName = string.Empty;
        public string TargetSceneName
        {
            get => _targetSceneName;
            set => _targetSceneName = value ?? string.Empty;
        }
    }
}
