#nullable enable
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Enums;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Systems
{
    /// <summary>
    /// Sorts into the Render phase and checks for an active camera.
    /// Later this will iterate visible renderables (MeshRenderer, SpriteRenderer, etc).
    /// Supports camera layer mask culling to skip rendering objects that don't match the camera's culling mask.
    /// </summary>
    public class RenderingSystem : SystemBase
    {
        #region Fields
        private Scene _scene = null!;
        private EngineContext _context = null!;
        private GraphicsDevice _device = null!;
        private List<MeshRenderer> _renderers = new List<MeshRenderer>(0);
        private Camera? _camera;
        #endregion

        #region Constructors
        public RenderingSystem()
            : base(FrameLifecycle.Render, order: 0)
        {
        }
        #endregion

        #region Lifecycle Methods
        protected override void OnAdded()
        {
            if (Scene == null)
                throw new NullReferenceException(nameof(Scene));

            // Cache for fast access
            _scene = Scene;
            _context = Scene.Context;
            _device = _context.GraphicsDevice;
        }

        public override void Draw(float deltaTime)
        {
            _renderers = _scene.Renderers;
            _camera = _scene.ActiveCamera;

            if (_renderers == null || _camera == null)
                return;

            int count = _renderers.Count;
            LayerMask cullingMask = _camera.CullingMask;

            for (int i = 0; i < count; i++)
            {
                MeshRenderer renderer = _scene.Renderers[i];

                // Skip rendering if the GameObject's layer doesn't overlap with the camera's culling mask
                if (renderer.GameObject != null && !cullingMask.Overlaps(renderer.GameObject.Layer))
                    continue;

                renderer.Render(_device, _camera);
            }
        }
        #endregion
    }
}