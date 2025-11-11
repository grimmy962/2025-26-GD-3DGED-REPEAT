using GDEngine.Core.Entities;
using GDEngine.Core.Enums;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using GDEngine.Core.Systems.Base;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Systems.Draw
{
    public class UIMenuSystem : SystemBase
    {
        private Scene _scene = null!;
        private EngineContext _context = null!;
        private GraphicsDevice _device = null!;
        private CameraSystem? _cameraSystem = null!;
        private readonly List<UIRenderer> _drawables = new List<UIRenderer>(16);

        public UIMenuSystem() 
            : base(FrameLifecycle.PostRender, 1000)
        {
        }

        protected override void OnAdded()
        {
            if (Scene == null)
                throw new NullReferenceException(nameof(Scene));

            _scene = Scene;
            _context = _scene.Context;
            _device = _context.GraphicsDevice;

            _cameraSystem = _scene.GetSystem<CameraSystem>();
        }

        public void Add(UIRenderer uiMenuRenderer)
        {
            if (uiMenuRenderer == null)
                throw new ArgumentNullException(nameof(uiMenuRenderer));
            if (_drawables.Contains(uiMenuRenderer))
                return;
            _drawables.Add(uiMenuRenderer);
        }

        public override void Draw(float deltaTime)
        {
            var camera = _cameraSystem?.ActiveCamera;
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            for (int i = 0; i < _drawables.Count; i++)
                if (_drawables[i].Enabled)
                    _drawables[i].Draw(_device, camera);
        }
    }
}
