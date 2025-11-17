using GDEngine.Core.Components;
using GDEngine.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Rendering
{
    /// <summary>
    /// Named UI layers for SpriteBatch BackToFront sorting (0 = front, 1 = back).
    /// Use directly as a float thanks to implicit conversion: ui.LayerDepth = UILayer.Menu;
    /// </summary>
    public readonly struct UILayer
    {
        #region Fields
        public readonly float Depth;
        #endregion

        #region Static Fields
        public static readonly UILayer Cursor = new UILayer(0f);          // on-top pointers/reticles
        public static readonly UILayer MenuFront = new UILayer(0.05f);    // highlights/selection states
        public static readonly UILayer Menu = new UILayer(0.1f);          // menu text/buttons
        public static readonly UILayer HUD = new UILayer(0.4f);           // in-game HUD overlays
        public static readonly UILayer Background = new UILayer(1f);      // background images / backdrops
        #endregion

        #region Constructors
        public UILayer(float depth)
        {
            Depth = depth;
        }
        #endregion

        #region Operator Overloading
        public static implicit operator float(UILayer layer)
        {
            return layer.Depth;
        }
        #endregion

        #region Housekeeping Methods
        public override string ToString()
        {
            return Depth.ToString("0.00");
        }
        #endregion
    }

    /// <summary>
    /// Base component for on-screen overlays that draw in PostRender via <see cref="UIRenderSystem"/>.
    /// Centralizes common UI draw fields so subclasses only supply per-type data (origin, scale, etc.).
    /// </summary>
    /// <see cref="Component"/>
    /// <see cref="UIRenderSystem"/>
    public abstract class UIRenderer : Component
    {
        #region Static Fields
        protected static readonly Vector2 _shadowNudge = new Vector2(1f, 1f);
        public const float LAYER_DEPTH_EPSILON = 0.1f; 
        #endregion

        #region Fields
        protected SpriteBatch? _spriteBatch;

        private float _layerDepth = 0.9f;
        private float _rotationRadians = 0f;
        private SpriteEffects _effects = SpriteEffects.None;

        private TextAnchor _anchor = TextAnchor.TopLeft;
        #endregion

        #region Properties
        public float LayerDepth
        {
            get { return _layerDepth; }
            set { _layerDepth = MathHelper.Clamp(value, 0f, 1f); }
        }

        public float RotationRadians
        {
            get { return _rotationRadians; }
            set { _rotationRadians = value; }
        }

        public SpriteEffects Effects
        {
            get { return _effects; }
            set { _effects = value; }
        }

        /// <summary>
        /// Logical anchor to use when positioning this UI element relative to its
        /// base position or rectangle (TopLeft, Center, BottomRight, etc).
        /// </summary>
        public TextAnchor Anchor
        {
            get { return _anchor; }
            set { _anchor = value; }
        }
        #endregion

        #region Helper Methods

        public static float Behind(float layerDepth, float e = LAYER_DEPTH_EPSILON)
        {
            return Math.Clamp(layerDepth + e, 0, 1);
        }

        public static float Before(float layerDepth, float e = LAYER_DEPTH_EPSILON)
        {
            return Math.Clamp(layerDepth - e, 0, 1);
        }

        /// <summary>
        /// Returns an offset from the top-left of a region of the given size so that
        /// drawing with this offset as the origin will respect the chosen anchor.
        /// (Same logic that used to live in <see cref="UITextRenderer"/>.)
        /// </summary>
        protected static Vector2 ComputeAnchorOffset(Vector2 size, TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.TopLeft: return Vector2.Zero;
                case TextAnchor.Top: return new Vector2(size.X * 0.5f, 0f);
                case TextAnchor.TopRight: return new Vector2(size.X, 0f);
                case TextAnchor.Left: return new Vector2(0f, size.Y * 0.5f);
                case TextAnchor.Center: return size * 0.5f;
                case TextAnchor.Right: return new Vector2(size.X, size.Y * 0.5f);
                case TextAnchor.BottomLeft: return new Vector2(0f, size.Y);
                case TextAnchor.Bottom: return new Vector2(size.X * 0.5f, size.Y);
                default: return new Vector2(size.X, size.Y); // BottomRight
            }
        }

        /// <summary>
        /// Convenience helper: given a base position and content size, returns the
        /// actual draw position and origin to use for DrawString/Draw.
        /// </summary>
        protected void ApplyAnchor(
            Vector2 basePosition,
            Vector2 contentSize,
            out Vector2 drawPosition,
            out Vector2 originFromAnchor)
        {
            originFromAnchor = ComputeAnchorOffset(contentSize, _anchor);
            drawPosition = basePosition;
        }
        #endregion

        #region Constructors
        protected UIRenderer()
        {
        }
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();

            // Cache SpriteBatch from the scene context
            _spriteBatch = GameObject?.Scene?.Context.SpriteBatch;

            // Auto-register with the scene's UIRenderSystem so it can be drawn
            var scene = GameObject?.Scene;
            if (scene == null)
                return;

            var uiSystem = scene.GetSystem<UIRenderSystem>();
            if (uiSystem != null)
                uiSystem.Add(this);
        }

        public abstract void Draw(GraphicsDevice device, Camera? camera);
        #endregion
    }

    public enum TextAnchor
    {
        TopLeft, Top, TopRight,
        Left, Center, Right,
        BottomLeft, Bottom, BottomRight
    }
}
