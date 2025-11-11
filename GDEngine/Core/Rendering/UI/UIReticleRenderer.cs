using GDEngine.Core.Components;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDEngine.Core.Rendering.UI
{
    /// <summary>
    /// Draws a rotating reticle sprite at the mouse position (with optional offset/scale),
    /// supporting texture atlases via <see cref="SourceRectangle"/>.
    /// </summary>
    /// <see cref="UIRenderer"/>
    public class UIReticleRenderer : UIRenderer
    {
        #region Static Fields
        private static readonly RasterizerState _raster = RasterizerState.CullNone;
        private static readonly DepthStencilState _depth = DepthStencilState.None;
        private static readonly BlendState _blend = BlendState.AlphaBlend;
        private static readonly SamplerState _sampler = SamplerState.PointClamp;
        #endregion

        #region Fields
        private SpriteBatch _spriteBatch;
        private Texture2D _texture;
        private Rectangle? _sourceRect;
        private Vector2 _origin;
        private Vector2 _scale = Vector2.One;
        private Vector2 _offset = Vector2.Zero;
        private float _rotationRad;
        private float _rotationSpeedDegPerSec = 90f;
        private float _layerDepth = 0f;
        private Color _tint = Color.White;
        #endregion

        #region Properties
        public Texture2D Texture { get => _texture; set { _texture = value; RecenterOriginFromSource(); } }
        public Rectangle? SourceRectangle { get => _sourceRect; set { _sourceRect = value; RecenterOriginFromSource(); } }
        public Vector2 Scale { get => _scale; set => _scale = value; }
        public Vector2 Offset { get => _offset; set => _offset = value; }
        public float RotationSpeedDegPerSec { get => _rotationSpeedDegPerSec; set => _rotationSpeedDegPerSec = value; }
        public float LayerDepth { get => _layerDepth; set => _layerDepth = MathHelper.Clamp(value, 0f, 1f); }
        public Color Tint { get => _tint; set => _tint = value; }
        #endregion

        #region Constructors
        public UIReticleRenderer(Texture2D texture, Rectangle? source = null)
        {
            _texture = texture;
            _sourceRect = source;
            RecenterOriginFromSource();
        }
        #endregion

        #region Methods
        public void RecenterOriginFromSource()
        {
            if (_texture == null) return;

            if (_sourceRect.HasValue)   //Rectangle(512,512,512,512)
            {
                var r = _sourceRect.Value;
                _origin = new Vector2(r.Width * 0.5f, r.Height * 0.5f);
            }
            else
            {
                _origin = new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f);
            }
        }
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();

            if (GameObject == null || GameObject.Scene == null)
                throw new NullReferenceException("Something is null!");

            _spriteBatch = GameObject.Scene.Context.SpriteBatch;
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            if (_spriteBatch == null || _texture == null) 
                return;

            _rotationRad += MathHelper.ToRadians(_rotationSpeedDegPerSec) * Time.DeltaTimeSecs;

            var mouse = Mouse.GetState().Position.ToVector2();
            var pos = mouse + _offset;

            _spriteBatch.Begin(SpriteSortMode.Deferred, _blend, 
                _sampler, _depth, _raster);

            _spriteBatch.Draw(_texture, pos, _sourceRect, _tint,  
                _rotationRad, _origin, _scale, SpriteEffects.None, _layerDepth);
            _spriteBatch.End();
        }
        #endregion
    }
}