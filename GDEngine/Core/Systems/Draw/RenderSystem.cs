#nullable enable
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Enums;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using GDEngine.Core.Systems.Base;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Systems
{
    /// <summary>
    /// Renders the scene for all cameras each frame.
    /// - Iterates cameras in stack order (Base -> Overlay, then by Depth).
    /// - For each camera: sets the device viewport from PixelViewport, applies camera clear, and renders visible renderers.
    /// - Restores the full backbuffer viewport at the end so UI/post systems can assume full-screen.
    /// </summary>
    public class RenderSystem : SystemBase
    {
        #region Fields
        private Scene _scene = null!;
        private EngineContext _context = null!;
        private GraphicsDevice _device = null!;
        private CameraSystem? _cameraSystem = null!;
        private readonly List<Camera> _cameraStack = new List<Camera>(8);
        private readonly List<MeshRenderer> _visible = new List<MeshRenderer>(512);
        #endregion

        #region Constructors
        public RenderSystem(int order = -100)
            : base(FrameLifecycle.Render, order: 0)
        {
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
            // No renderables? early out
            var renderers = _scene.Renderers;
            if (renderers == null || renderers.Count == 0)
                throw new ArgumentNullException(nameof(renderers));

            if (_cameraSystem == null)
                throw new ArgumentNullException(nameof(_cameraSystem));

            _cameraSystem.GetSortedStack(_cameraStack);

            if (_cameraStack.Count == 0)
                return;

            // Keep a copy of the full backbuffer viewport so we can restore it later
            var fullViewport = _device.Viewport;

            // For each camera: set viewport, clear (if any), filter by mask, then draw
            for (int i = 0; i < _cameraStack.Count; i++)
            {
                var camera = _cameraStack[i];

                // Apply this camera's viewport 
                _device.Viewport = camera.GetViewport(_device);

                // Clear per camera (for overlays, ClearFlags is typically None)
                _cameraSystem.ApplyClears(camera);

                // Build visible set (mask + later bounds test)
                _cameraSystem.BuildVisibleSet(camera, renderers, _visible);

                // Render each visible renderer with this camera
                for (int j = 0; j < _visible.Count; j++)
                {
                    _visible[j].Draw(_device, camera);
                }
            }

            // Restore full-screen viewport so UI/Post systems behave as expected
            _device.Viewport = fullViewport;
        }
        #endregion
    }
}
