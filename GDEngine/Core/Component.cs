using Microsoft.Xna.Framework;
using System;

namespace GDEngine.Core
{
    /// <summary>
    /// Base class for most game entities (not systems)
    /// e.g. Camera, Transform, Rotator, CameraRotator
    /// Enemy [patrol, animation, sound, activation]
    /// Pickup [transform, meshfilter, meshrenderer, 
    //                  opacityCycle, rotation, translation]
    /// </summary>
    public class Component
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

        public void InternalAwake() {
            if (!Enabled)
            {
                Enabled = true;
                Awake();
            }
        }
        public void InternalStart() 
        { 
            if(Enabled && !HasStarted)
            {
                HasStarted = true;
                Start();
            }
        }
        public void InternalUpdate(GameTime gameTime) 
        {
            if (Enabled && HasStarted)
                Update(gameTime);
        }
        public void InternalLateUpdate(GameTime gameTime) 
        {
            if (Enabled && HasStarted)
                LateUpdate(gameTime);
        }
        public void InternalOnDestroy() 
        {
            if (Enabled && HasStarted)
                OnDestroy();
        }


        //TODO - add code to do internal start etc
    }
}
