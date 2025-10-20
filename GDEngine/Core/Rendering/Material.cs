using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core
{
    /// <summary>
    /// Describes how a mesh is drawn: shader (Effect), technique, and fixed-function render states.
    /// Provides a light-weight parameter cache so you can set shader uniforms once per frame or object.
    /// </summary>
    public sealed class Material
    {
        #region Fields
        private Effect _effect;
        private string _techniqueName = string.Empty;

        private BlendState _blendState = BlendState.Opaque;
        private DepthStencilState _depthStencilState = DepthStencilState.Default;
        private RasterizerState _rasterizerState = RasterizerState.CullCounterClockwise;
        private SamplerState _samplerState = SamplerState.LinearWrap;

        //TODO - map
        #endregion

        #region Properties
        public Effect Effect
        {
            get => _effect;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _effect = value;
            }
        }

        public string TechniqueName
        {
            get => _techniqueName;
            set => _techniqueName = value ?? string.Empty;
        }

        public BlendState BlendState
        {
            get => _blendState;
            set => _blendState = value ?? BlendState.Opaque;
        }

        public DepthStencilState DepthStencilState
        {
            get => _depthStencilState;
            set => _depthStencilState = value ?? DepthStencilState.Default;
        }

        public RasterizerState RasterizerState
        {
            get => _rasterizerState;
            set => _rasterizerState = value ?? RasterizerState.CullCounterClockwise;
        }

        public SamplerState SamplerState
        {
            get => _samplerState;
            set => _samplerState = value ?? SamplerState.LinearWrap;
        }
        #endregion

        #region Constructors
        public Material(Effect effect)
        {
            _effect = effect ?? throw new ArgumentNullException(nameof(effect));
            //TODO - map
        }
        #endregion


        #region Constructors
        #endregion

        #region Methods
        public void Apply(GraphicsDevice device)
        {
            //Set render states

            //Set technique

            //Set parameters

            //Foreach pass
             //Apply
             //Draw
        }
        #endregion

        #region Lifecycle Methods
        #endregion

        #region Housekeeping Methods
        #endregion
    }
}
