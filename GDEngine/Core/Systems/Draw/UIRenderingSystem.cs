using GDEngine.Core.Entities;
using GDEngine.Core.Enums;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Systems
{
    /// <summary>
    /// PostRender dispatcher that asks overlay components to draw after the 3D pass.
    /// Assumes RenderingSystem restored the full backbuffer viewport.
    /// </summary>
    /// <see cref="Scene"/>
    /// <see cref="UIRenderer"/>
    /// <see cref="RenderingSystem"/>
    public sealed class UIRenderSystem : SystemBase
    {
        #region Fields
        private Scene _scene = null!;
        private EngineContext _context = null!;
        private GraphicsDevice _device = null!;
        private CameraSystem _cameraSystem = null!;
        private readonly List<UIRenderer> _drawables = new List<UIRenderer>(16);
        #endregion

        #region Constructors
        public UIRenderSystem(int order = 10)
            : base(FrameLifecycle.PostRender, order)
        {
        }
        #endregion

        #region Methods
        public void Add(UIRenderer uiOverlay)
        {
            if (uiOverlay == null)
                throw new ArgumentNullException(nameof(uiOverlay));
            if (_drawables.Contains(uiOverlay))
                return;
            _drawables.Add(uiOverlay);
        }

        public void Remove(UIRenderer uiOverlay)
        {
            if (uiOverlay == null)
                return;
            _drawables.Remove(uiOverlay);
        }
        #endregion

        #region Lifecycle Methods
        protected override void OnAdded()
        {
            if (Scene == null)
                throw new NullReferenceException(nameof(Scene));

            _scene = Scene;
            _context = _scene.Context;
            _device = _context.GraphicsDevice;
            _cameraSystem = _scene.GetSystem<CameraSystem>();
        }

        public override void Draw(float deltaTime)
        {
            var camera = _cameraSystem?.ActiveCamera;
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            for (int i = 0; i < _drawables.Count; i++)
                if (_drawables[i].Enabled)
                    _drawables[i].Render(_device, camera);
        }
        #endregion
    }
}
