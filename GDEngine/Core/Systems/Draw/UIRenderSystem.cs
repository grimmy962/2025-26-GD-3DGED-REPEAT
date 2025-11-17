using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Enums;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using GDEngine.Core.Systems.Base;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1.Effects;

namespace GDEngine.Core.Systems
{
    /// <summary>
    /// PostRender dispatcher that asks overlay components to draw after the 3D pass.
    /// Assumes RenderingSystem restored the full backbuffer viewport.
    /// </summary>
    /// <see cref="Scene"/>
    /// <see cref="UIRenderer"/>
    /// <see cref="RenderSystem"/>
    public sealed class UIRenderSystem : SystemBase
    {
        #region Static Fields
        private static readonly SpriteSortMode _sort = SpriteSortMode.BackToFront;
        private static readonly RasterizerState _raster = RasterizerState.CullCounterClockwise;
        private static readonly DepthStencilState _depth = DepthStencilState.None;
        private static readonly BlendState _blend = BlendState.AlphaBlend;
        private static readonly SamplerState _sampler = SamplerState.PointClamp;
        #endregion

        #region Fields
        private Scene _scene = null!;
        private EngineContext _context = null!;
        private GraphicsDevice _device = null!;
        private SpriteBatch _spriteBatch;
        private readonly List<UIRenderer> _renderers = new List<UIRenderer>(16);
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public UIRenderSystem(int order = 10)
            : base(FrameLifecycle.PostRender, order)
        {
  
        }
        #endregion

        #region Methods
        public void Add(UIRenderer renderer)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));
            if (_renderers.Contains(renderer))
                return;
            _renderers.Add(renderer);
        }

        public void Remove(UIRenderer renderer)
        {
            if (renderer == null)
                return;
            _renderers.Remove(renderer);
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
            _spriteBatch = _context.SpriteBatch;
        }

        public override void Draw(float deltaTime)
        {
          _spriteBatch.Begin(_sort, _blend, _sampler, _depth, _raster);

            for (int i = 0; i < _renderers.Count; i++)
                if (_renderers[i].Enabled)
                    _renderers[i].Draw(_device, null);

            _spriteBatch.End();
        }
        #endregion
    }
}
