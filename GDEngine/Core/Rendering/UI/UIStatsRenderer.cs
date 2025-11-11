#nullable enable
using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Rendering;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GDEngine.Core.Debug
{
    /// <summary>
    /// FPS + custom text lines overlay that draws in PostRender. Attach to a GameObject.
    /// Uses centralized batching via <see cref="UIRenderer"/>.
    /// </summary>
    public sealed class UIStatsRenderer : UIRenderer
    {
        #region Fields
        private readonly CircularBuffer<float> _recentDt = new CircularBuffer<float>(60);
        private SpriteFont _font = null!;
        private Vector2 _anchor = new Vector2(5, 5);
        private Color _shadow = Color.Black;
        private Color _text = Color.Yellow;
        private System.Func<IEnumerable<string>>? _linesProvider;
        private GraphicsDevice? _graphicsDevice;
        private Texture2D? _backgroundTexture;
        private Color _bgColor = new Color(40, 40, 40, 125); // grey with alpha
        private Vector2 _texturePadding = new Vector2(5f, 5f);
        private float _headerTemplateWidth;
        private string _header = string.Empty;
        private Rectangle _backRect;
        private float _gapAfterHeader;
        private System.Collections.Generic.List<string>? _extra;
        #endregion

        #region Properties
        public Vector2 Anchor { get => _anchor; set => _anchor = value; }
        public Color Shadow { get => _shadow; set => _shadow = value; }
        public Color TextColor { get => _text; set => _text = value; }
        public System.Func<IEnumerable<string>>? LinesProvider { get => _linesProvider; set => _linesProvider = value; }
        public SpriteFont Font { get => _font; set => _font = value; }
        public Color BackgroundColor { get => _bgColor; set => _bgColor = value; }
        public Vector2 TexturePadding { get => _texturePadding; set => _texturePadding = value; }
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();

            // Generate a background texture that is 1x1 and white
            _graphicsDevice = GameObject?.Scene?.Context.GraphicsDevice;
            if (_graphicsDevice != null && _backgroundTexture == null)
            {
                _backgroundTexture = new Texture2D(_graphicsDevice, 1, 1, false, SurfaceFormat.Color);
                _backgroundTexture.SetData(new[] { Color.White }); // tint with background color at draw time
            }

            if (_font != null)
            {
                string template = "FPS: 0000.0  |  Render: 00.00 ms  |  Uptime: 000000s";
                _headerTemplateWidth = _font.MeasureString(template).X;
            }
        }

        protected override void LateUpdate(float deltaTime)
        {
            float dt = MathF.Max(Time.UnscaledDeltaTimeSecs, 1e-6f);
            _recentDt.Push(dt);

            var arr = _recentDt.ToArray();
            float sum = 0f;
            for (int i = 0; i < arr.Length; i++) sum += arr[i];
            float avgDt = arr.Length > 0 ? sum / arr.Length : dt;

            float fps = avgDt > 0f ? 1f / avgDt : 0f;
            float ms = avgDt * 1000f;
            _header = $"FPS: {fps:0.0}  | Render: {ms:0.00} ms  |  Uptime: {Time.RealtimeSinceStartupSecs,6:F2}s";

            int linesCount = 1;
            float maxWidth = _headerTemplateWidth;

            _extra = null;
            if (_linesProvider != null)
            {
                _extra = new System.Collections.Generic.List<string>();
                foreach (var line in _linesProvider())
                {
                    _extra.Add(line);
                    float w = _font.MeasureString(line).X;
                    if (w > maxWidth) maxWidth = w;
                }
                linesCount += _extra.Count;
            }

            _gapAfterHeader = (_extra != null && _extra.Count > 0) ? 5f : 0f;
            float totalHeight = _texturePadding.Y * 2f + _font.LineSpacing * linesCount + _gapAfterHeader;
            float totalWidth = _texturePadding.X * 2f + maxWidth;

            _backRect = new Rectangle(
                (int)System.MathF.Floor(_anchor.X - _texturePadding.X),
                (int)System.MathF.Floor(_anchor.Y - _texturePadding.Y),
                (int)System.MathF.Ceiling(totalWidth),
                (int)System.MathF.Ceiling(totalHeight)
            );
        }

        public override void Draw(GraphicsDevice device, Camera? camera)
        {
            if (_spriteBatch == null || _font == null) return;

            var dropShadowLayerDepth = Behind(LayerDepth);

            // Background (slightly behind text)
            if (_backgroundTexture != null)
                _spriteBatch.Draw(_backgroundTexture, _backRect, null, _bgColor, 0f, Vector2.Zero, SpriteEffects.None, dropShadowLayerDepth);

            _spriteBatch.DrawString(_font, _header, _anchor + _shadowNudge, _shadow, RotationRadians, Vector2.Zero, 1f, Effects, dropShadowLayerDepth);
            _spriteBatch.DrawString(_font, _header, _anchor, TextColor, RotationRadians, Vector2.Zero, 1f, Effects, LayerDepth);

            // Extra lines
            float y = _anchor.Y + _font.LineSpacing + _gapAfterHeader;
            if (_extra != null)
            {
                for (int i = 0; i < _extra.Count; i++)
                {
                    var pos = new Vector2(_anchor.X, y);
                    _spriteBatch.DrawString(_font, _extra[i], pos + _shadowNudge, _shadow, RotationRadians, Vector2.Zero, 1f, Effects, dropShadowLayerDepth);
                    _spriteBatch.DrawString(_font, _extra[i], pos, TextColor, RotationRadians, Vector2.Zero, 1f, Effects, LayerDepth);
                    y += _font.LineSpacing;
                }
            }
        }
        #endregion
    }
}
