using GDEngine.Core.Components;
using GDEngine.Core.Enums;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using GDEngine.Core.Systems.Base;

namespace GDEngine.Core.Entities
{
    /// <summary>
    /// Logical collection of scene GameObject instances and lifecycle-ordered systems.
    /// Coordinates component lifecycle (Awake/Start/Update/LateUpdate) and dispatches system lifecycles.
    /// </summary>
    /// <see cref="GameObject"/>
    /// <see cref="Component"/>
    /// <see cref="SystemBase"/>
    /// <see cref="FrameLifecycle"/>
    /// <see cref="EngineContext"/>
    public sealed class Scene : IDisposable
    {
        #region Fields
        // Owned objects
        private readonly List<GameObject> _gameObjects = new();

        // Lifecycle tracking used to wake up new components
        private readonly HashSet<Component> _started = new();

        // Flat snapshot for inspection/UI
        private readonly List<SystemBase> _systemsAll = new();

        // Systems bucketed by FrameLifecycle index (we know there are exactly 5 lifecycles)
        private readonly List<SystemBase>[] _systemsByLifecycle;

        private List<MeshRenderer> _renderers = new List<MeshRenderer>(512);
        public List<MeshRenderer> Renderers { get => _renderers; set => _renderers = value; }

        private readonly EngineContext _context;

        private bool _disposed = false;

        #endregion

        #region Properties
        public string Name { get; set; }

        public EngineContext Context => _context;

        // Camera selection; to be owned by CameraSystem later
        public Camera? ActiveCamera { get; set; }

        public IReadOnlyList<GameObject> GameObjects => _gameObjects;
        public IReadOnlyList<SystemBase> Systems => _systemsAll;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="Scene"/>.
        /// </summary>
        /// <param name="context">Engine services container used by the scene.</param>
        /// <param name="name">Debug/display name.</param>
        public Scene(EngineContext context, string name = "Untitled Scene")
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Name = name;

            _systemsByLifecycle = new List<SystemBase>[5];
            for (int i = 0; i < _systemsByLifecycle.Length; i++)
                _systemsByLifecycle[i] = new List<SystemBase>(4);
        }
        #endregion

        #region Core Methods
        /// <summary>
        /// Adds a system to the scene; routes to the lifecycle bucket and sorts by Order within that bucket.
        /// </summary>
        public void Add(SystemBase system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            if (system.Scene != this && system.Scene != null)
                throw new InvalidOperationException("System already attached to a different Scene.");

            system.OnAddedToScene(this);
            _systemsAll.Add(system);

            var systemBucket = _systemsByLifecycle[(int)system.Lifecycle];
            systemBucket.Add(system);

            // Stable ascending order by Order
            systemBucket.Sort((a, b) =>
            {
                if (a.Order == b.Order)
                    return 0;
                return a.Order < b.Order ? -1 : 1;
            });
        }

        /// <summary>
        /// Adds an existing <see cref="GameObject"/> to the scene and runs Awake() on its components.
        /// </summary>
        public GameObject Add(GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            if (_gameObjects.Contains(gameObject))
                return gameObject;

            gameObject.Scene = this;

            _gameObjects.Add(gameObject);

            //get all renderers for this game objects
            var renderers = gameObject.GetComponents<MeshRenderer>();
            //add all the renderers from this game object
            if (renderers != null && renderers.Count > 0)
                _renderers.AddRange(renderers);

            // Run Awake on all pre-existing components
            var comps = gameObject.Components;
            for (int i = 0; i < comps.Count; i++)
                comps[i].InternalAwake();

            // Promote first enabled camera if none set yet (temporary until CameraSystem)
            var cam = gameObject.GetComponent<Camera>();
            if (ActiveCamera == null && cam != null && cam.Enabled)
                ActiveCamera = cam;

            return gameObject;
        }

        /// <summary>
        /// Finds the first GameObject matching the predicate
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public GameObject? Find(Predicate<GameObject> filter)
        {
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                var go = _gameObjects[i];
                if (filter(go))
                    return go;
            }

            return null;
        }

        public List<GameObject>? FindAll(Predicate<GameObject> filter)
        {
            List<GameObject> found = new List<GameObject>(8);

            for (int i = 0; i < _gameObjects.Count; i++)
            {
                var go = _gameObjects[i];
                if (filter(go))
                    found.Add(go);
            }

            return found.Count == 0 ? null : found;
        }




        /// <summary>
        /// Returns a specific system if one is already added; otherwise returns null.
        /// </summary>
        public T? GetSystem<T>() where T : SystemBase
        {
            for (int i = 0; i < _systemsAll.Count; i++)
            {
                if (_systemsAll[i] is T t)
                    return t;
            }
            return null;
        }


        /// <summary>
        /// Advances non-render lifecycles and drives component lifecycle. Call once per frame.
        /// </summary>
        public void Update(float deltaTime)
        {
            // The last two lifecycles are Render and PostRender; skip them here.
            var nonRenderCount = _systemsByLifecycle.Length - 2;

            // Run all non-render system lifecycles in index order
            for (int li = 0; li < nonRenderCount; li++)
            {
                var systemBucket = _systemsByLifecycle[li];
                for (int i = 0; i < systemBucket.Count; i++)
                {
                    var s = systemBucket[i];
                    if (!s.Enabled)
                        continue;
                    s.Update(deltaTime);
                }
            }

            // Ensure Start() runs once per component
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                var go = _gameObjects[i];
                if (!go.Enabled)
                    continue;

                var components = go.Components;
                for (int j = 0; j < components.Count; j++)
                {
                    var c = components[j];
                    if (_started.Contains(c))
                        continue;

                    c.InternalStart();
                    _started.Add(c);
                }
            }

            // Update GameObject pass
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                var go = _gameObjects[i];
                if (!go.Enabled)
                    continue;

                var components = go.Components;
                for (int j = 0; j < components.Count; j++)
                    components[j].InternalUpdate(deltaTime);
            }

            // LateUpdate GameObject pass
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                var gameObject = _gameObjects[i];
                if (!gameObject.Enabled)
                    continue;

                var components = gameObject.Components;
                for (int j = 0; j < components.Count; j++)
                    components[j].InternalLateUpdate(deltaTime);
            }
        }

        /// <summary>
        /// Dispatches Render and PostRender lifecycles in order.
        /// </summary>
        public void Draw(float deltaTime)
        {
            var renderSystems = _systemsByLifecycle[(int)FrameLifecycle.Render];
            for (int i = 0; i < renderSystems.Count; i++)
            {
                var system = renderSystems[i];
                if (!system.Enabled)
                    continue;
                system.Draw(deltaTime);
            }

            var postRenderSystems = _systemsByLifecycle[(int)FrameLifecycle.PostRender];
            for (int i = 0; i < postRenderSystems.Count; i++)
            {
                var system = postRenderSystems[i];
                if (!system.Enabled)
                    continue;
                system.Draw(deltaTime);
            }
        }
        #endregion

        #region Lifecycle Methods
        // None
        #endregion

        #region Housekeeping Methods
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Dispose all GameObjects and their components
                for (int i = 0; i < _gameObjects.Count; i++)
                {
                    var go = _gameObjects[i];
                    var components = go.Components;

                    // Dispose components that implement IDisposable
                    for (int j = 0; j < components.Count; j++)
                    {
                        if (components[j] is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }

                    go.Destroy();
                }

                // Dispose systems that implement IDisposable
                for (int i = 0; i < _systemsAll.Count; i++)
                {
                    if (_systemsAll[i] is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                Clear();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Scene()
        {
            Dispose(false);
        }

        /// <summary>
        /// Removes all objects and systems from the scene and clears lifecycle state.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _gameObjects.Count; i++)
                _gameObjects[i].Destroy();

            _gameObjects.Clear();
            _renderers.Clear();
            _started.Clear();
            _systemsAll.Clear();

            for (int i = 0; i < _systemsByLifecycle.Length; i++)
                _systemsByLifecycle[i].Clear();

            ActiveCamera = null;
        }

        public void ClearGameObject()
        {
            for (int i = 0; i < _gameObjects.Count; i++)
                _gameObjects[i].Destroy();

            _gameObjects.Clear();
            _renderers.Clear();
            _started.Clear();
        }

        /// <summary>
        /// String for diagnostics.
        /// </summary>
        public override string ToString()
        {
            return $"Scene(Name={Name}, GameObjects={_gameObjects.Count}, Systems={_systemsAll.Count})";
        }
        #endregion
    }
}
