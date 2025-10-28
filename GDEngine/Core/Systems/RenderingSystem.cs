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
    /// </summary>
    public class RenderingSystem : SystemBase
    {
        private Scene _scene;
        private EngineContext _context;
        private GraphicsDevice _device;
        private List<MeshRenderer> _renderers;
        private Camera? _camera;

        public RenderingSystem()
            : base(FrameLifecycle.Render, order: 0)
        {
  
        }

        protected override void OnAdded()
        {
            //cache for fast access
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

            for (int i = 0; i < count; i++)
                _scene.Renderers[i].Render(_device, _camera);
        }
    }
}

