using GDEngine.Core.Components;
using GDEngine.Core.Rendering.Base;
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
        public static readonly UILayer Cursor = new UILayer(0f); // on-top pointers/reticles
        public static readonly UILayer MenuFront = new UILayer(0.05f); // highlights/selection states
        public static readonly UILayer Menu = new UILayer(0.1f); // menu text/buttons
        public static readonly UILayer HUD = new UILayer(0.4f); // in-game HUD overlays
        public static readonly UILayer Background = new UILayer(1f); // background images / backdrops
        #endregion

        #region Constructors
        public UILayer(float depth) { Depth = depth; }
        #endregion

        #region Operator Overloading
        public static implicit operator float(UILayer layer) => layer.Depth;
        #endregion

        #region Housekeeping Methods
        public override string ToString() => Depth.ToString("0.00");
        #endregion
    }

    /// <summary>
    /// Base component for on-screen overlays that draw in PostRender via <see cref="UIRenderSystem"/>.
    /// Centralizes common UI draw fields so subclasses only supply per-type data (origin, scale, etc.).
    /// </summary>
    /// <see cref="Component"/>
    /// <see cref="UIRenderSystem"/>
    public class UIRenderer : Component, IDraw
    {
        #region Static Fields
        protected static readonly Vector2 _shadowNudge = new Vector2(1f, 1f);
        #endregion

        #region Fields
        protected UIRenderSystem? _uiRenderSystem;
        protected SpriteBatch? _spriteBatch;     // shared batch from EngineContext
        private float _layerDepth = 0.1f;   // default mid-layer
        private float _rotationRadians = 0f;
        private SpriteEffects _effects = SpriteEffects.None;
        #endregion

        #region Properties
        /// <summary>Layer depth for BackToFront sorting (0 = in front, 1 = back).</summary>
        public float LayerDepth { get => _layerDepth; set => _layerDepth = MathHelper.Clamp(value, 0f, 1f); }
        /// <summary>Common rotation (radians) for both text and textures.</summary>
        public float RotationRadians { get => _rotationRadians; set => _rotationRadians = value; }
        /// <summary>Common flip flags for SpriteBatch (applies to both text and textures).</summary>
        public SpriteEffects Effects { get => _effects; set => _effects = value; }
        #endregion

        #region Helper Methods
        public const float LAYER_DEPTH_EPSILON = 1E-2f; //0.01f;
        public static float Behind(float layerDepth, float e = LAYER_DEPTH_EPSILON)
        {
            return Math.Clamp(layerDepth + e, 0, 1);
        }

        public static float Before(float layerDepth, float e = LAYER_DEPTH_EPSILON) //0.01f
        {
            return Math.Clamp(layerDepth - e, 0, 1);
        } 
        #endregion


        #region Methods
        /// <summary>Subclasses issue SpriteBatch draw calls here. Never call SpriteBatch.Begin/End inside components.</summary>
        public virtual void Draw(GraphicsDevice device, Camera? camera) { }
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            var scene = GameObject?.Scene;
            if (scene == null)
                throw new NullReferenceException("UIRenderer requires a GameObject in a Scene.");

            _uiRenderSystem = scene.GetSystem<UIRenderSystem>()
                ?? throw new InvalidOperationException("UIRenderSystem not found. Add it to the Scene before using UIRenderer.");

            _uiRenderSystem.Add(this);
            _spriteBatch = scene.Context.SpriteBatch;
        }

        protected override void OnDestroy()
        {
            _uiRenderSystem?.Remove(this);
            _uiRenderSystem = null;
            _spriteBatch = null;
        }
        #endregion
    }
}
