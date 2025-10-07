using Microsoft.Xna.Framework;

namespace GDEngine.Core
{
    public abstract class Component
    {
        public bool HasStarted { get; private set; }
        public bool IsDestroyed { get; private set; }
        public bool Enabled { get; set; } = true;

        protected virtual void Awake()
        {
        }
        protected virtual void Start()
        {
        }
        protected virtual void Update(GameTime gameTime)
        {
        }
        protected virtual void LateUpdate(GameTime gameTime)
        {
        }
        protected virtual void OnDestroy()
        {
        }

       //TODO - add code to do internal start etc
    }
}
