#nullable enable
using GDEngine.Core.Components;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDEngine.Core.Rendering.UI
{
    /// <summary>
    /// Draws a rotating reticle sprite at the screen centre (with optional offset/scale).
    /// Uses centralized batching in <see cref="UIRenderer"/>.
    /// </summary>
    public class UIReticleRenderer : UIRenderer
    {
        #region Fields
        private Texture2D _texture = null!;
        private Rectangle? _sourceRect;
        private Vector2 _origin;
        private Vector2 _scale = Vector2.One;
        private Vector2 _offset = Vector2.Zero;
        private float _rotationSpeedDegPerSec = 90f;
        private Color _tint = Color.White;
        #endregion

        #region Constructors
        public UIReticleRenderer(Texture2D texture)
        {
            _texture = texture;
        }
        #endregion

        #region Properties
        public Texture2D Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                RecenterOriginFromSource();
            }
        }

        public Rectangle? SourceRectangle
        {
            get => _sourceRect;
            set
            {
                _sourceRect = value;
                RecenterOriginFromSource();
            }
        }

        public Vector2 Scale
        {
            get => _scale;
            set => _scale = value;
        }

        /// <summary>
        /// Optional 2D offset from the screen centre in pixels.
        /// </summary>
        public Vector2 Offset
        {
            get => _offset;
            set => _offset = value;
        }

        public float RotationSpeedDegPerSec
        {
            get => _rotationSpeedDegPerSec;
            set => _rotationSpeedDegPerSec = value;
        }

        public Color Tint
        {
            get => _tint;
            set => _tint = value;
        }
        #endregion

        #region Methods
        public void RecenterOriginFromSource()
        {
            if (_texture == null)
                return;

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
        protected override void Update(float deltaTime)
        {
            // Advance base RotationRadians so we use the centralized rotation field.
            RotationRadians += MathHelper.ToRadians(_rotationSpeedDegPerSec) * Time.DeltaTimeSecs;
        }

        public override void Draw(GraphicsDevice device, Camera? camera)
        {
            if (_spriteBatch == null || _texture == null)
                return;

            var mouse = Mouse.GetState().Position.ToVector2();
            var pos = mouse + _offset;

            _spriteBatch.Draw(
                _texture,
                pos,
                _sourceRect,
                _tint,
                RotationRadians,
                _origin,
                _scale,
                Effects,
                LayerDepth);
        }
        #endregion
    }
}
