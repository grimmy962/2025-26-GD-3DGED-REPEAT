using GDEngine.Core.Collections;
using GDEngine.Core.Enums;
using GDEngine.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core
{
    public class PerfStatsSystem : SystemBase
    {
        // Fields
        private readonly Vector2 _pos = new Vector2(8, 6);
        private readonly SpriteFont _font;
        private SpriteBatch _spriteBatch;

        private Func<IEnumerable<string>>? _linesProvider;

        // Optional: tiny smoothing window (we'll describe the class in the aside)
        private readonly CircularBuffer<float> _recentDt = new CircularBuffer<float>(60);

        public PerfStatsSystem(SpriteFont font, Func<IEnumerable<string>>? linesProvider = null)
            : base(FrameLifecycle.PostRender, order: 10)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
            _linesProvider = linesProvider;
        }

        protected override void OnAdded()
        {
            var ctx = Context ?? throw new InvalidOperationException("EngineContext not set.");
            _spriteBatch = ctx.SpriteBatch;
        }

        public override void Draw(float deltaTime)
        {
            // Use unscaled delta so timescale changes don't affect FPS readout.
            float dt = MathF.Max(Timing.Time.UnscaledDeltaTimeSecs, 1e-6f); //16.67ms
            _recentDt.Push(dt);

            // Simple average over the small window
            var arr = _recentDt.ToArray();
            float sum = 0f;
            for (int i = 0; i < arr.Length; i++) sum += arr[i];
            float avgDt = arr.Length > 0 ? sum / arr.Length : dt;

            float fps = avgDt > 0f ? 1f / avgDt : 0f;
            float ms = avgDt * 1000f;
            string text = $"FPS: {fps:0.0}  |  {ms:0.00} ms  |  Frames: {Timing.Time.FrameCount}";

            _spriteBatch.Begin();
            
            _spriteBatch.DrawString(_font, text, _pos + new Vector2(2,2), Color.Black);
            _spriteBatch.DrawString(_font, text, _pos, Color.Yellow);

            float y = _pos.Y + _font.LineSpacing + 5f;
            if (_linesProvider != null)
            {
                foreach (var line in _linesProvider())
                {
                    _spriteBatch.DrawString(_font, line, new Vector2(_pos.X, y) + new Vector2(2,2), Color.Black);
                    _spriteBatch.DrawString(_font, line, new Vector2(_pos.X, y), Color.Yellow);
                    y += _font.LineSpacing;
                }
            }
            _spriteBatch.End();

            
        }
    }
}
