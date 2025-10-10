
using Microsoft.Xna.Framework;

namespace GDEngine.Core
{
    /// <summary>
    /// Store for all systems and game objects
    /// </summary>
    public class Scene
    {
        private EngineContext _context;
        private readonly List<SystemBase> _systems;
        private List<GameObject> _objects;

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
            _objects = new List<GameObject>();
        }

        public GameObject AddGameObject(string name)
        {
            name = name.Trim().ToLower(); // "dylan"
            var gameObject = new GameObject(name);
            _objects.Add(gameObject);

            foreach(var c in gameObject.Components)
            {
                if (!c.Enabled)
                    c.InternalAwake(); //TODO - check awake?
            }

            return gameObject;
        }

        public void DestroyGameObject(GameObject gameObject)
        {
            if(_objects.Remove(gameObject))
            {
                foreach (var c in gameObject.Components)
                    c.InternalDestroy();
            }
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
        public void Update(GameTime gameTime)  //camera, sound, physics, CD/CR
        {
            foreach (var system in _systems)
                system.Update();

            //update
            foreach(var obj in _objects)
            {
                //for each component
                foreach(var c in obj.Components)
                {
                    if(c.Enabled)
                        c.InternalUpdate(gameTime);
                }
            }

            //late update
            foreach (var obj in _objects)
            {
                //for each component
                foreach (var c in obj.Components)
                {
                    if (c.Enabled)
                        c.InternalLateUpdate(gameTime);
                }
            }
        }
        public void Draw()  //rendering, UI
        {
            //call Draw on RenderingSystem -> draws all MeshRenderers
            foreach (var system in _systems)
                system.Draw();
        }
    }
}
