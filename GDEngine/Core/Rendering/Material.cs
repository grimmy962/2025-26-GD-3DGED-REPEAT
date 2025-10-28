using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Rendering
{
    /// <summary>
    /// Minimal wrapper around an <see cref="Effect"/>.
    /// Holds render states and exposes a single Apply that works for all Effect types.
    /// </summary>
    public sealed class Material
    {
        #region Static Fields
        #endregion

        #region Fields
        private readonly Effect _effect;
        private RenderStates.RenderStateBlock _stateBlock;
        private SamplerState _samplerState = SamplerState.LinearWrap;
        #endregion

        #region Properties
        public Effect Effect => _effect;

        public RenderStates.RenderStateBlock StateBlock
        {
            get => _stateBlock;
            set => _stateBlock = value;
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
            _stateBlock = RenderStates.Default3D();
        }
        #endregion

        #region Methods
        public void SetTechnique(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (_effect.Techniques[name] != null)
                _effect.CurrentTechnique = _effect.Techniques[name];
        }

        public void Apply(GraphicsDevice device, Matrix world, Matrix view, Matrix projection, EffectPropertyBlock block, Action drawCall)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (drawCall == null) throw new ArgumentNullException(nameof(drawCall));

            // Render states
            _stateBlock.Apply(device);
            if (_samplerState != null)
                device.SamplerStates[0] = _samplerState;

            // Effect binding via registry
            var binder = EffectBinderRegistry.Find(_effect);
            binder.ApplyCommonMatrices(_effect, world, view, projection);
            if (block != null) binder.Apply(_effect, block);

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
