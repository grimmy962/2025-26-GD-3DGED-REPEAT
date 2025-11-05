#nullable enable
using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Rendering;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Debug
{
    /// <summary>
    /// FPS + custom text lines overlay that draws in PostRender. Attach to a GameObject.
    /// </summary>
    /// <see cref="UIRenderer"/>
    public sealed class UIStatsRenderer : UIRenderer
    {
        #region Fields
        private readonly CircularBuffer<float> _recentDt = new CircularBuffer<float>(60);
        private SpriteFont _font = null!;
        private Vector2 _anchor = new Vector2(5, 5);
        private Color _shadow = Color.Black;
        private Color _text = Color.Yellow;
        private Func<IEnumerable<string>>? _linesProvider;
        private SpriteBatch? _spriteBatch;
        private GraphicsDevice? _graphicsDevice;
        private Texture2D? _backgroundTexture;
        private Color _bgColor = new Color(40, 40, 40, 125); // grey with alpha
        private Vector2 _texturePadding = new Vector2(5f, 5f);
        private float _headerTemplateWidth;
        #endregion

        #region Properties
        public Vector2 Anchor { get => _anchor; set => _anchor = value; }
        public Color Shadow { get => _shadow; set => _shadow = value; }
        public Color TextColor { get => _text; set => _text = value; }
        public Func<IEnumerable<string>>? LinesProvider { get => _linesProvider; set => _linesProvider = value; }
        public SpriteFont Font { get => _font; set => _font = value; }
        public Color BackgroundColor { get => _bgColor; set => _bgColor = value; }
        public Vector2 TexturePadding { get => _texturePadding; set => _texturePadding = value; }

        #endregion

        #region Constructors
        public UIStatsRenderer() { }

        public UIStatsRenderer(SpriteFont font, Func<IEnumerable<string>>? linesProvider = null)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
            _linesProvider = linesProvider;
        }
        #endregion

        #region Methods
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();

            // Get ref to draw textures and strings
            _spriteBatch = GameObject?.Scene?.Context.SpriteBatch;

            // Generate a background texture that is 1x1 and white
            _graphicsDevice = GameObject?.Scene?.Context.GraphicsDevice;
            if (_graphicsDevice != null && _backgroundTexture == null)
            {
                _backgroundTexture = new Texture2D(_graphicsDevice, 1, 1, false, SurfaceFormat.Color);
                _backgroundTexture.SetData(new[] { Color.White }); // tint with background color at draw time
            }


            // In Awake()
            if (_font != null)
            {
                string template = "FPS: 0000.0  |  Render: 00.00 ms  |  Uptime: 000000s"; // adjust zeros to your expected max
                _headerTemplateWidth = _font.MeasureString(template).X;
            }
        }
        #endregion

        #region Housekeeping Methods
        public override void Render(GraphicsDevice device, Camera camera)
        {
            if (_spriteBatch == null || _font == null)
                return;

            // Unscaled delta so timescale doesn't change the FPS readout.
            float dt = MathF.Max(Time.UnscaledDeltaTimeSecs, 1e-6f);
            _recentDt.Push(dt);

            var arr = _recentDt.ToArray();
            float sum = 0f;
            for (int i = 0; i < arr.Length; i++) sum += arr[i];
            float avgDt = arr.Length > 0 ? sum / arr.Length : dt;

            float fps = avgDt > 0f ? 1f / avgDt : 0f;
            float ms = avgDt * 1000f;
            string header = $"FPS: {fps:0.0}  | Render: {ms:0.00} ms  |  Uptime: {Time.RealtimeSinceStartupSecs,6:F2}s";

            // Build the lines we draw so we can measure the needed background size.
            // First line is the header, then optional custom lines.
            int linesCount = 1;
            float maxWidth = _headerTemplateWidth;

            // We’ll re-enumerate once for drawing; keep measurement pass minimal.
            List<string>? extra = null;
            if (_linesProvider != null)
            {
                extra = new List<string>();
                foreach (var line in _linesProvider())
                {
                    extra.Add(line);
                    float w = _font.MeasureString(line).X;
                    if (w > maxWidth) maxWidth = w;
                }
                linesCount += extra.Count;
            }

            // Height: one header + N extra lines, with a small gap after the header.
            float gapAfterHeader = (extra != null && extra.Count > 0) ? 5f : 0f;
            float totalHeight = _texturePadding.Y * 2f + _font.LineSpacing * linesCount + gapAfterHeader;
            float totalWidth = _texturePadding.X * 2f + maxWidth;

            var backRect = new Rectangle(
                (int)MathF.Floor(_anchor.X - _texturePadding.X),
                (int)MathF.Floor(_anchor.Y - _texturePadding.Y),
                (int)MathF.Ceiling(totalWidth),
                (int)MathF.Ceiling(totalHeight)
            );

            // Draw
            _spriteBatch.Begin();

            // Background (semi-transparent grey)
            if (_backgroundTexture != null)
                _spriteBatch.Draw(_backgroundTexture, backRect, _bgColor);

            // Header text (shadow + text)
            _spriteBatch.DrawString(_font, header, _anchor + new Vector2(2, 2), _shadow);
            _spriteBatch.DrawString(_font, header, _anchor, _text);

            // Extra lines
            float y = _anchor.Y + _font.LineSpacing + gapAfterHeader;
            if (extra != null)
            {
                for (int i = 0; i < extra.Count; i++)
                {
                    var pos = new Vector2(_anchor.X, y);
                    _spriteBatch.DrawString(_font, extra[i], pos + new Vector2(2, 2), _shadow);
                    _spriteBatch.DrawString(_font, extra[i], pos, _text);
                    y += _font.LineSpacing;
                }
            }

            _spriteBatch.End();
        }

        #endregion
    }
}
