using GDEngine.Core.Entities;
using System;

namespace GDEngine.Core.Components
{
    /// <summary>
    /// Base class for attachable behaviour with a Unity-like lifecycle.
    /// </summary>
    /// <see cref="GameObject"/>
    /// <see cref="Transform"/>
    public abstract class Component
    {
        #region Fields
        // Lifecycle flags
        private bool _isAwake;     // true after Awake() has run once
        private bool _isStarted;   // true after Start() has run once
        private bool _isDestroyed; // true after OnDestroy() has run
        private bool _wasEnabled;  // tracks previous enabled state for OnEnabled/OnDisabled
        #endregion

        #region Properties
        private bool _enabled = true;

        /// <summary>
        /// If false, Update/LateUpdate will be skipped for this component.
        /// Setting this property triggers OnEnabled() or OnDisabled() callbacks.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value || _isDestroyed)
                    return;

                bool wasEnabled = _enabled;
                _enabled = value;

                // Only fire callbacks after Awake has run
                if (_isAwake)
                {
                    if (_enabled && !wasEnabled)
                    {
                        OnEnabled();
                    }
                    else if (!_enabled && wasEnabled)
                    {
                        OnDisabled();
                    }
                }

                _wasEnabled = _enabled;
            }
        }

        // The owning GameObject; set when the component is attached.
        public GameObject? GameObject { get; private set; }

        // Convenience reference to the owner's Transform.
        public Transform? Transform { get; private set; }
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// Attaches this component to a GameObject and wires common references.
        /// </summary>
        /// <param name="gameObject">The GameObject to attach to.</param>
        internal void Attach(GameObject gameObject)
        {
            if (_isDestroyed)
                throw new ObjectDisposedException(GetType().Name);
            GameObject = gameObject ?? throw new ArgumentNullException(nameof(gameObject));
            Transform = gameObject.Transform ?? throw new InvalidOperationException("GameObject must have a Transform.");
        }

        /// <summary>
        /// Internal entry point that ensures Awake() runs once.
        /// </summary>
        internal void InternalAwake()
        {
            if (_isDestroyed)
                return; // don't awaken destroyed components
            if (_isAwake)
                return;

            Awake();
            _isAwake = true;
            _wasEnabled = _enabled;

            // If the component is enabled at awake time, call OnEnabled
            if (_enabled)
            {
                OnEnabled();
            }
        }

        /// <summary>
        /// Internal entry point that ensures Start() runs once (after Awake()).
        /// </summary>
        internal void InternalStart()
        {
            if (_isDestroyed)
                return;    // don't start destroyed components
            if (!_isAwake)
                InternalAwake();
            if (_isStarted)
                return;

            if (Enabled && GameObject?.Enabled == true)
                Start();

            _isStarted = true;
        }

        /// <summary>
        /// Internal per-frame Update call (skipped if disabled or destroyed).
        /// </summary>
        /// <param name="deltaTime">Time since last frame in seconds.</param>
        internal void InternalUpdate(float deltaTime)
        {
            if (_isDestroyed)
                return;
            if (!Enabled || GameObject?.Enabled != true)
                return;

            Update(deltaTime);
        }

        /// <summary>
        /// Internal per-frame LateUpdate call (runs after Update; skipped if disabled or destroyed).
        /// </summary>
        /// <param name="deltaTime">Time since last frame in seconds.</param>
        internal void InternalLateUpdate(float deltaTime)
        {
            if (_isDestroyed)
                return;
            if (!Enabled || GameObject?.Enabled != true)
                return;

            LateUpdate(deltaTime);
        }

        /// <summary>
        /// Internal destruction hook to clean up resources and mark this component as destroyed.
        /// </summary>
        internal void InternalDestroy()
        {
            if (_isDestroyed)
                return;

            // Call OnDisabled if component was enabled before destruction
            if (_enabled && _isAwake)
            {
                OnDisabled();
            }

            OnDestroy();

            // Clear references to aid GC and prevent accidental reuse.
            GameObject = null;
            Transform = null;
            _enabled = false;

            _isDestroyed = true;
        }

        /// <summary>
        /// Called once when the component is first created/added or the scene loads.
        /// </summary>
        protected virtual void Awake() { }

        /// <summary>
        /// Called once before the first Update after Awake().
        /// </summary>
        protected virtual void Start() { }

        /// <summary>
        /// Called every frame when both this component and its GameObject are enabled.
        /// </summary>
        /// <param name="deltaTime">Time since last frame in seconds.</param>
        protected virtual void Update(float deltaTime) { }

        /// <summary>
        /// Called every frame after Update() when enabled.
        /// </summary>
        /// <param name="deltaTime">Time since last frame in seconds.</param>
        protected virtual void LateUpdate(float deltaTime) { }

        /// <summary>
        /// Called when the component becomes enabled and active.
        /// This is called after Awake() if the component starts enabled,
        /// and whenever Enabled is set from false to true.
        /// </summary>
        protected virtual void OnEnabled() { }

        /// <summary>
        /// Called when the component becomes disabled.
        /// This is called whenever Enabled is set from true to false,
        /// and before OnDestroy() if the component was enabled when destroyed.
        /// Use this to unsubscribe from events or pause behavior.
        /// </summary>
        protected virtual void OnDisabled() { }

        /// <summary>
        /// Called once when the component is being destroyed or the scene is unloading.
        /// Use to release unmanaged resources or unsubscribe from events.
        /// Note: OnDisabled() is called before this if the component was enabled.
        /// </summary>
        protected virtual void OnDestroy() { }
        #endregion
    }
}