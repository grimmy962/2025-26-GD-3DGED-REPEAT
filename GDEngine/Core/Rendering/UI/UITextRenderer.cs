using GDEngine.Core.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDEngine.Core.Rendering.UI
{
    /// <summary>
    /// Generic UI text renderer that draws a string at a position supplied by a delegate.
    /// Use it for mouse-locked labels, fixed HUD text, or dynamically positioned UI.
    /// </summary>
    /// <see cref="UIReticleRenderer"/>
    public class UITextRenderer : UIRenderer
    {
        #region Static Fields
        private static readonly RasterizerState _raster = RasterizerState.CullNone;
        private static readonly DepthStencilState _depth = DepthStencilState.None;
        private static readonly BlendState _blend = BlendState.AlphaBlend;
        private static readonly SamplerState _sampler = SamplerState.PointClamp;
        private static readonly Vector2 _shadowNudge = new Vector2(1f, 1f);
        #endregion

        #region Fields
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        private Func<string> _textProvider = () => string.Empty;
        private Func<Vector2> _positionProvider = () => Vector2.Zero;
        private Func<Color> _colorProvider = null;

        private Vector2 _offset = Vector2.Zero;
        private float _scale = 1f;
        private float _layerDepth = 0f;
        private bool _dropShadow = true;
        private Color _fallbackColor = Color.White;
        private Color _shadowColor = new Color(0, 0, 0, 180);
        private TextAnchor _anchor = TextAnchor.TopLeft;
        #endregion

        #region Properties
        public SpriteFont Font { get => _font; set => _font = value; }
        public Func<string> TextProvider { get => _textProvider; set => _textProvider = value ?? (() => string.Empty); }
        public Func<Vector2> PositionProvider { get => _positionProvider; set => _positionProvider = value ?? (() => Vector2.Zero); }
        public Func<Color> ColorProvider { get => _colorProvider; set => _colorProvider = value; }
        public Vector2 Offset { get => _offset; set => _offset = value; }
        public float Scale { get => _scale; set => _scale = Math.Max(0.01f, value); }
        public float LayerDepth { get => _layerDepth; set => _layerDepth = MathHelper.Clamp(value, 0f, 1f); }
        public bool DropShadow { get => _dropShadow; set => _dropShadow = value; }
        public Color FallbackColor { get => _fallbackColor; set => _fallbackColor = value; }
        public Color ShadowColor { get => _shadowColor; set => _shadowColor = value; }
        public TextAnchor Anchor { get => _anchor; set => _anchor = value; }
        #endregion

        #region Constructors
        public UITextRenderer(SpriteFont font) { _font = font; }

        public UITextRenderer(SpriteFont font, string text, Vector2 position)
        {
            _font = font;
            _textProvider = () => text ?? string.Empty;
            _positionProvider = () => position;
        }

        public static UITextRenderer FromMouse(SpriteFont font, string text)
        {
            return new UITextRenderer(font)
            {
                _textProvider = () => text ?? string.Empty,
                _positionProvider = () => Mouse.GetState().Position.ToVector2()
            };
        }
        #endregion

        #region Methods
        public static Vector2 ComputeAnchorOffset(Vector2 size, TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.TopLeft: return Vector2.Zero;
                case TextAnchor.Top: return new Vector2(size.X * 0.5f, 0);
                case TextAnchor.TopRight: return new Vector2(size.X, 0);
                case TextAnchor.Left: return new Vector2(0, size.Y * 0.5f);
                case TextAnchor.Center: return size * 0.5f;
                case TextAnchor.Right: return new Vector2(size.X, size.Y * 0.5f);
                case TextAnchor.BottomLeft: return new Vector2(0, size.Y);
                case TextAnchor.Bottom: return new Vector2(size.X * 0.5f, size.Y);
                default: return new Vector2(size.X, size.Y); // BottomRight
            }
        }
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();
            _spriteBatch = GameObject.Scene.Context.SpriteBatch;
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            if (_spriteBatch == null || _font == null) return;

            var text = _textProvider?.Invoke() ?? string.Empty;
            if (text.Length == 0) return;

            var basePos = _positionProvider?.Invoke() ?? Vector2.Zero;
            var size = _font.MeasureString(text) * _scale;
            var anchorOff = ComputeAnchorOffset(size, _anchor);
            var drawPos = basePos + _offset - anchorOff;

            var color = _colorProvider != null ? _colorProvider() : _fallbackColor;

            _spriteBatch.Begin(SpriteSortMode.Deferred, _blend, _sampler, _depth, _raster);
            if (_dropShadow)
                _spriteBatch.DrawString(_font, text, drawPos + _shadowNudge, _shadowColor, 0f, Vector2.Zero, _scale, SpriteEffects.None, _layerDepth);
            _spriteBatch.DrawString(_font, text, drawPos, color, 0f, Vector2.Zero, _scale, SpriteEffects.None, _layerDepth);
            _spriteBatch.End();
        }
        #endregion
    }

    public enum TextAnchor
    {
        TopLeft, Top, TopRight,
        Left, Center, Right,
        BottomLeft, Bottom, BottomRight
    }
}