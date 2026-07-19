
using GDEngine.Core.Entities;
using GDGame.Zones.Shared;

namespace GDGame.Zones
{
    public interface IZoneBuilder
    {
        string SceneName
        { 
            get;
        }

        Scene Build(ZoneBuildContext buildContext);
    }
}
