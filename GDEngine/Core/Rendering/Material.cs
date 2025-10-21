using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GDEngine.Core
{
    /// <summary>
    /// Minimal wrapper around an <see cref="Effect"/> for teaching.
    /// Lets you set a technique and a few parameters, then apply and draw.
    /// </summary>
    public sealed class Material
    {
        #region Static Fields
        #endregion

        #region Fields
        private Effect _effect;
        private string _techniqueName = string.Empty;

        private BlendState _blendState = BlendState.Opaque;
        private DepthStencilState _depthStencilState = DepthStencilState.Default;
        private RasterizerState _rasterizerState = RasterizerState.CullCounterClockwise;
        private SamplerState _samplerState = SamplerState.LinearWrap;
        #endregion

        #region Properties
        public Effect Effect => _effect;

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

        /// <summary>
        /// Snapshot of render states for easy copy/paste between materials.
        /// </summary>
        public RenderStates.RenderStateBlock StateBlock
        {
            get
            {
                return new RenderStates.RenderStateBlock(_blendState, _depthStencilState, _rasterizerState, _samplerState);
            }
            set
            {
                _blendState = value.Blend ?? BlendState.Opaque;
                _depthStencilState = value.DepthStencil ?? DepthStencilState.Default;
                _rasterizerState = value.Rasterizer ?? RasterizerState.CullCounterClockwise;
                _samplerState = value.Sampler ?? SamplerState.LinearWrap;
            }
        }
        #endregion

        #region Constructors
        public Material(Effect effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            _effect = effect;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets World/View/Projection if present, and WorldViewProj if present.
        /// </summary>
        public void SetMatrices(Matrix world, Matrix view, Matrix projection)
        {
            var pWorld = _effect.Parameters["World"];
            if (pWorld != null) 
                pWorld.SetValue(world);

            var pView = _effect.Parameters["View"];
            if (pView != null) 
                pView.SetValue(view);

            var pProjection = _effect.Parameters["Projection"];
            if (pProjection != null) 
                pProjection.SetValue(projection);

            var pWVP = _effect.Parameters["WorldViewProj"];
            if (pWVP != null) 
                pWVP.SetValue(world * view * projection);
        }

        public void SetFloat(string name, float value)
        {
            var p = _effect.Parameters[name];
            if (p != null) 
                p.SetValue(value);
        }

        public void SetVector3(string name, Vector3 value)
        {
            var p = _effect.Parameters[name];
            if (p != null) 
                p.SetValue(value);
        }

        public void SetVector4(string name, Vector4 value)
        {
            var p = _effect.Parameters[name];
            if (p != null) 
                p.SetValue(value);
        }

        public void SetColor(string name, Color value)
        {
            var p = _effect.Parameters[name];
            if (p != null) 
                p.SetValue(value.ToVector4());
        }

        public void SetMatrix(string name, Matrix value)
        {
            var p = _effect.Parameters[name];
            if (p != null) 
                p.SetValue(value);
        }

        public void SetTexture(string name, Texture2D value)
        {
            var p = _effect.Parameters[name];
            if (p != null) 
                p.SetValue(value);
        }

        /// <summary>
        /// Sets render states, optional technique, applies passes, and runs drawCall once per pass.
        /// </summary>
        public void Apply(GraphicsDevice device, Action drawCall)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (drawCall == null)
                throw new ArgumentNullException(nameof(drawCall));

            device.BlendState = _blendState;
            device.DepthStencilState = _depthStencilState;
            device.RasterizerState = _rasterizerState;
            device.SamplerStates[0] = _samplerState;

            if (!string.IsNullOrEmpty(_techniqueName))
                _effect.CurrentTechnique = _effect.Techniques[_techniqueName];

            var passes = _effect.CurrentTechnique.Passes;
            for (int i = 0; i < passes.Count; i++)
            {
                var pass = passes[i];
                pass.Apply();
                drawCall();
            }
        }
        #endregion

        #region Lifecycle Methods
        #endregion

        #region Housekeeping Methods
        #endregion
    }
}
