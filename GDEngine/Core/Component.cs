using Microsoft.Xna.Framework;

namespace GDEngine.Core
{
    /// <summary>
    /// Base class for the components found in a game object (e.g. Camera, Transform, Rotator, CameraRotator)
    /// In an Enemy we have: transform, meshfilter, meshrenderer, patrol controller
    /// In a Pickup we have: transform, camera controller
    /// </summary>
    public class Component
    {
        public GameObject GameObject { get; set; }
        public Transform Transform => GameObject.Transform;
        public bool Enabled { get; set; } = true;
        private bool _hasStarted = false;
        private bool _isDestroyed = false;

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update(GameTime gameTime) { }
        protected virtual void LateUpdate(GameTime gameTime) { }
        protected virtual void OnDestroy() { }

        public void InternalAwake()
        {
            Awake();
        }
        public void InternalStart()
        {
            if (!_hasStarted)
            {
                Start();
                _hasStarted = true;
            }
        }
        public void InternalUpdate(GameTime gameTime) 
        { 
            if (Enabled && !_isDestroyed) 
                Update(gameTime); 
        }
        public void InternalLateUpdate(GameTime gameTime) 
        { 
            if (Enabled && !_isDestroyed) 
                LateUpdate(gameTime); 
        }
        public void InternalDestroy()
        {
            if (!_isDestroyed)
            {
                OnDestroy();
                _isDestroyed = true;
            }
        }
    }
}
