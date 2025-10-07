
namespace GDEngine.Core
{
    public class Scene
    {
        private EngineContext _context;
        private readonly List<SystemBase> _systems;
        private Camera _activeCamera;

        public EngineContext Context
        {
            get
            {
                return _context;
            }
        }
        public Camera ActiveCamera
        {
            get { return _activeCamera; }
            set { _activeCamera = value;}
        }
        public Scene(EngineContext context)
        {
            _context = context;
            _systems = new List<SystemBase>();
        }
    
        public void AddSystem(SystemBase system)
        {
            system.Scene = this;
            _systems.Add(system);
            //TODO - call added to scene
        }
        public void RemoveSystem(SystemBase system)
        {
            if(_systems.Remove(system))
            {
                //TODO - set the scene ref in system to null
            }
        }
        public void Update()  //camera, sound, physics, CD/CR
        {
            foreach (var system in _systems)
                system.Update();
        }
        public void Draw()  //rendering, UI
        {
            foreach (var system in _systems)
                system.Draw();
        }
    }
}
