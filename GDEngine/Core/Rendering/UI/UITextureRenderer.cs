using GDEngine.Core.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Rendering.UI
{
    /// <summary>
    /// Draws a Texture2D in screen space via centralized batching in <see cref="UIRenderer"/>.
    /// </summary>
    public class UITextureRenderer : UIRenderer
    {
        #region Fields
        private Texture2D? _texture;
        private Rectangle? _sourceRect;
        private Vector2 _position;
        private Vector2 _origin;
        private Vector2 _scale = Vector2.One;
        private Color _tint = Color.White;
        #endregion

        #region Properties
        public Texture2D? Texture { get => _texture; set => _texture = value; }
        public Rectangle? SourceRectangle { get => _sourceRect; set => _sourceRect = value; }
        public Vector2 Position { get => _position; set => _position = value; }
        public Vector2 Origin { get => _origin; set => _origin = value; }
        public Vector2 Scale { get => _scale; set => _scale = value; }
        public Color Tint { get => _tint; set => _tint = value; }
        #endregion

        #region Methods
        public void CenterOrigin()
        {
            if (_texture == null) return;
            if (_sourceRect.HasValue)
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
        public override void Draw(GraphicsDevice device, Camera? camera)
        {
            if (_spriteBatch == null || _texture == null) return;

            _spriteBatch.Draw(_texture, _position, _sourceRect, _tint,
                RotationRadians, _origin, _scale, Effects, LayerDepth);
        }
        #endregion
    }
}
