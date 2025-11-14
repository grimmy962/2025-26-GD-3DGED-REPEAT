using GDEngine.Core.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDEngine.Core.Rendering.UI
{
    /// <summary>
    /// Generic UI text renderer that draws a string at a position supplied by a delegate.
    /// Uses centralized batching via <see cref="UIRenderer"/>.
    /// </summary>
    public class UITextRenderer : UIRenderer
    {
        #region Fields
        private SpriteFont _font = null!;

        private Func<string>? _textProvider = () => string.Empty;
        private Func<Vector2>? _positionProvider = () => Vector2.Zero;
        private Func<Color>? _colorProvider = null;

        private Vector2 _offset = Vector2.Zero;
        private float _scale = 1f;
        private bool _dropShadow = true;
        private Color _fallbackColor = Color.White;
        private Color _shadowColor = new Color(0, 0, 0, 180);

        private string _text = string.Empty;
        private Vector2 _size;
        private Vector2 _originFromAnchor;
        private Vector2 _drawPos;
        private Color _resolvedColor;
        #endregion

        #region Properties
        public SpriteFont Font { get => _font; set => _font = value; }
        public Func<string>? TextProvider { get => _textProvider; set => _textProvider = value ?? (() => string.Empty); }
        public Func<Vector2>? PositionProvider { get => _positionProvider; set => _positionProvider = value ?? (() => Vector2.Zero); }
        public Func<Color>? ColorProvider { get => _colorProvider; set => _colorProvider = value; }
        public Vector2 Offset { get => _offset; set => _offset = value; }
        public float Scale { get => _scale; set => _scale = Math.Max(0.01f, value); }
        public bool DropShadow { get => _dropShadow; set => _dropShadow = value; }
        public Color FallbackColor { get => _fallbackColor; set => _fallbackColor = value; }
        public Color ShadowColor { get => _shadowColor; set => _shadowColor = value; }
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

        #endregion

        #region Lifecycle Methods
        protected override void LateUpdate(float deltaTime)
        {
            _text = _textProvider?.Invoke() ?? string.Empty;
            if (_text.Length == 0) return;

            var basePos = _positionProvider?.Invoke() ?? Vector2.Zero;
            _size = _font.MeasureString(_text) * _scale;

            // Use base helper
            _originFromAnchor = ComputeAnchorOffset(_size, Anchor);

            _drawPos = basePos + _offset;

            _resolvedColor = _colorProvider != null ? _colorProvider() : _fallbackColor;
        }


        public override void Draw(GraphicsDevice device, Camera? camera)
        {
            if (_spriteBatch == null || _font == null || _text.Length == 0) return;

            if (_dropShadow)
                _spriteBatch.DrawString(_font, _text, _drawPos + _shadowNudge, _shadowColor,
                    RotationRadians, _originFromAnchor, _scale, Effects, Behind(LayerDepth));

            _spriteBatch.DrawString(_font, _text, _drawPos, _resolvedColor,
                RotationRadians, _originFromAnchor, _scale, Effects, LayerDepth);
        }
        #endregion
    }
}
