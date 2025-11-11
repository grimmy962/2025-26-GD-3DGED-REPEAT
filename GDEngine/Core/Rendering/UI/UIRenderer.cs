using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Systems;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Rendering
{
    /// <summary>
    /// Base component for on-screen overlays that draw in PostRender via <see cref="UIRenderSystem"/>.
    /// Attach this to a <see cref="GameObject"/>; it auto-registers with the system.
    /// </summary>
    /// <see cref="Component"/>
    /// <see cref="UIRenderSystem"/>
    public class UIRenderer : Component, IDraw
    {
        #region Fields
        protected UIRenderSystem? _uiRenderSystem;
        #endregion

        #region Methods
        /// <summary>
        /// Override to draw your overlay using the supplied device and camera.
        /// </summary>
        public virtual void Draw(GraphicsDevice device, Camera camera) { }
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            var scene = GameObject?.Scene;
            if (scene == null)
                throw new NullReferenceException("OverlayRenderer requires a GameObject in a Scene.");

            _uiRenderSystem = scene.GetSystem<UIRenderSystem>();
            if (_uiRenderSystem == null)
                throw new InvalidOperationException("OverlayRenderSystem not found. Add it to the Scene before using OverlayRenderer.");

            _uiRenderSystem.Add(this);
        }

        protected override void OnDestroy()
        {
            _uiRenderSystem?.Remove(this);
            _uiRenderSystem = null;
        }

        #endregion
    }
}
