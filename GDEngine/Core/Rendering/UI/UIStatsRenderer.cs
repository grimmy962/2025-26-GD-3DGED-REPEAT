#nullable enable
using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Rendering;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1;
using System.Collections.Generic;

namespace GDEngine.Core.Debug
{
    /// <summary>
    /// FPS + custom text lines overlay that draws in PostRender. Attach to a GameObject.
    /// Uses centralized batching via <see cref="UIRenderer"/>.
    /// Renders in a chosen screen corner with a configurable margin.
    /// </summary>
    public sealed class UIStatsRenderer : UIRenderer
    {
        #region Fields
        private readonly CircularBuffer<float> _recentDt = new CircularBuffer<float>(60);

        private SpriteFont _font = null!;
        private GraphicsDevice? _graphicsDevice;
        private Texture2D? _backgroundTexture;

        // Layout
        private Vector2 _anchor = new Vector2(5f, 5f);        // top-left of header text
        private Vector2 _texturePadding = new Vector2(5f, 5f);
        private Rectangle _backRect;
        private float _headerTemplateWidth;
        private float _gapAfterHeader;

        // Style
        private Color _shadow = Color.Black;
        private Color _text = Color.Yellow;
        private Color _bgColor = new Color(40, 40, 40, 125);  // grey with alpha

        // Content
        private string _header = string.Empty;
        private System.Func<IEnumerable<string>>? _linesProvider;
        private List<string>? _extra;

        // Corner anchoring
        private ScreenCorner _screenCorner = ScreenCorner.TopLeft;
        private Vector2 _margin = new Vector2(10f, 10f);
        #endregion

        #region Properties
        public SpriteFont Font
        {
            get { return _font; }
            set { _font = value; }
        }

        public Color TextColor
        {
            get { return _text; }
            set { _text = value; }
        }

        public Color Shadow
        {
            get { return _shadow; }
            set { _shadow = value; }
        }

        public Color BackgroundColor
        {
            get { return _bgColor; }
            set { _bgColor = value; }
        }

        public Vector2 TexturePadding
        {
            get { return _texturePadding; }
            set { _texturePadding = value; }
        }

        /// <summary>
        /// Function that provides additional debug lines to render under the FPS header.
        /// Called once per LateUpdate.
        /// </summary>
        public System.Func<IEnumerable<string>>? LinesProvider
        {
            get { return _linesProvider; }
            set { _linesProvider = value; }
        }

        /// <summary>
        /// Corner of the game window that this overlay should attach to.
        /// </summary>
        public ScreenCorner ScreenCorner
        {
            get { return _screenCorner; }
            set { _screenCorner = value; }
        }

        /// <summary>
        /// Margin in pixels from the chosen screen corner to the outer edge of the panel.
        /// </summary>
        public Vector2 Margin
        {
            get { return _margin; }
            set { _margin = value; }
        }
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();

            _graphicsDevice = GameObject?.Scene?.Context.GraphicsDevice;
            if (_graphicsDevice != null && _backgroundTexture == null)
            {
                _backgroundTexture = new Texture2D(_graphicsDevice, 1, 1, false, SurfaceFormat.Color);
                _backgroundTexture.SetData(new[] { Color.White });
            }

            if (_font != null)
            {
                const string template = "FPS: 0000.0  |  Render: 00.00 ms  |  Uptime: 000000s";
                _headerTemplateWidth = _font.MeasureString(template).X;
            }
        }

        protected override void LateUpdate(float deltaTime)
        {
            float dt = MathF.Max(Time.UnscaledDeltaTimeSecs, 1e-6f);
            _recentDt.Push(dt);

            var arr = _recentDt.ToArray();
            float sum = 0f;
            for (int i = 0; i < arr.Length; i++)
                sum += arr[i];

            float avgDt = arr.Length > 0 ? sum / arr.Length : dt;

            float fps = avgDt > 0f ? 1f / avgDt : 0f;
            float ms = avgDt * 1000f;

            _header = $"FPS: {fps:0.0}  |  Render: {ms:0.00} ms  |  Uptime: {Time.RealtimeSinceStartupSecs,6:F2}s";

            int linesCount = 1;
            float maxWidth = _headerTemplateWidth;

            _extra = null;
            int extraLineCount = 0;

            if (_linesProvider != null)
            {
                _extra = new List<string>();
                foreach (var line in _linesProvider())
                {
                    _extra.Add(line);
                    float width = _font.MeasureString(line).X;
                    if (width > maxWidth)
                        maxWidth = width;
                }
                extraLineCount = _extra.Count;
            }

            // Calculate gap separately from line count
            _gapAfterHeader = extraLineCount > 0 ? 10f : 0f;

            // Total height = padding + header + gap + extras + padding
            float totalHeight = _texturePadding.Y * 2f +              // Top padding
                               _font.LineSpacing +                     // Header line
                               _gapAfterHeader +                       // Gap
                               (_font.LineSpacing * extraLineCount);   // Extra lines
                                                                       // (bottom padding already in first term)

            float totalWidth = _texturePadding.X * 2f + maxWidth;

            // Work out where the panel should sit in the window
            Vector2 panelTopLeft = ComputePanelTopLeft(totalWidth, totalHeight);

            // Anchor is the top-left of the header text inside the panel
            _anchor = panelTopLeft + _texturePadding;

            _backRect = new Rectangle(
                (int)MathF.Floor(panelTopLeft.X),
                (int)MathF.Floor(panelTopLeft.Y),
                (int)MathF.Ceiling(totalWidth),
                (int)MathF.Ceiling(totalHeight));
        }

        public override void Draw(GraphicsDevice device, Camera? camera)
        {
            if (_spriteBatch == null || _font == null)
                return;

            float backgroundDepth = Behind(LayerDepth);

            // Background panel
            if (_backgroundTexture != null)
            {
                _spriteBatch.Draw(
                    _backgroundTexture,
                    _backRect,
                    null,
                    _bgColor,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    backgroundDepth);
            }

            // Header line
            _spriteBatch.DrawString(
                _font,
                _header,
                _anchor + _shadowNudge,
                _shadow,
                RotationRadians,
                Vector2.Zero,
                1f,
                Effects,
                backgroundDepth);

            _spriteBatch.DrawString(
                _font,
                _header,
                _anchor,
                _text,
                RotationRadians,
                Vector2.Zero,
                1f,
                Effects,
                LayerDepth);

            // Extra lines
            float y = _anchor.Y + _font.LineSpacing + _gapAfterHeader;
            if (_extra != null)
            {
                for (int i = 0; i < _extra.Count; i++)
                {
                    var pos = new Vector2(_anchor.X, y);

                    _spriteBatch.DrawString(
                        _font,
                        _extra[i],
                        pos + _shadowNudge,
                        _shadow,
                        RotationRadians,
                        Vector2.Zero,
                        1f,
                        Effects,
                        backgroundDepth);

                    _spriteBatch.DrawString(
                        _font,
                        _extra[i],
                        pos,
                        _text,
                        RotationRadians,
                        Vector2.Zero,
                        1f,
                        Effects,
                        LayerDepth);

                    y += _font.LineSpacing;
                }
            }
        }
        #endregion

        #region Methods
        private Vector2 ComputePanelTopLeft(float panelWidth, float panelHeight)
        {
            if (_graphicsDevice == null)
                return _margin; // Fallback: top-left with margin

            var vp = _graphicsDevice.Viewport;

            switch (_screenCorner)
            {
                case ScreenCorner.TopLeft:
                    return new Vector2(
                        _margin.X,
                        _margin.Y);

                case ScreenCorner.TopMiddle:
                    return new Vector2(
                        (vp.Width - panelWidth) * 0.5f,
                        _margin.Y);

                case ScreenCorner.TopRight:
                    return new Vector2(
                        vp.Width - panelWidth - _margin.X,
                        _margin.Y);

                case ScreenCorner.LeftMiddle:
                    return new Vector2(
                        _margin.X,
                        (vp.Height - panelHeight) * 0.5f);

                case ScreenCorner.Center:
                    return new Vector2(
                        (vp.Width - panelWidth) * 0.5f,
                        (vp.Height - panelHeight) * 0.5f);

                case ScreenCorner.RightMiddle:
                    return new Vector2(
                        vp.Width - panelWidth - _margin.X,
                        (vp.Height - panelHeight) * 0.5f);

                case ScreenCorner.BottomLeft:
                    return new Vector2(
                        _margin.X,
                        vp.Height - panelHeight - _margin.Y);

                case ScreenCorner.BottomMiddle:
                    return new Vector2(
                        (vp.Width - panelWidth) * 0.5f,
                        vp.Height - panelHeight - _margin.Y);

                case ScreenCorner.BottomRight:
                default:
                    return new Vector2(
                        vp.Width - panelWidth - _margin.X,
                        vp.Height - panelHeight - _margin.Y);
            }
        }
        #endregion
    }

    public enum ScreenCorner
    {
        TopLeft,
        TopMiddle,
        TopRight,
        LeftMiddle,
        Center,
        RightMiddle,
        BottomLeft,
        BottomMiddle,
        BottomRight
    }
}