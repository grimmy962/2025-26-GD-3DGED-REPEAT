using GDEngine.Core.Components;
using GDEngine.Core.Rendering.Base;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Rendering
{
    /// <summary>
    /// Draws a <see cref="MeshFilter"/> using a <see cref="Material"/>; supports per-object overrides via <see cref="EffectPropertyBlock"/>.
    /// </summary>
    /// <see cref="MeshFilter"/>
    /// <see cref="Material"/>
    /// <see cref="Camera"/>
    public sealed class MeshRenderer : Component, IDraw
    {
        #region Static Fields
        #endregion

        #region Fields
        private MeshFilter? _meshFilter;
        private Material? _material;
        private readonly EffectPropertyBlock _overrides = new();
        #endregion

        #region Properties
        public Material? Material
        {
            get => _material;
            set => _material = value;
        }

        public EffectPropertyBlock Overrides => _overrides;
        #endregion

        #region Constructors
        #endregion

        #region Methods
        public void Draw(GraphicsDevice device, Camera? camera)
        {
            if (Transform == null) return;
            if (_meshFilter == null) return;
            if (_material == null) return;
            if (camera == null) return;

            _meshFilter.BindBuffers(device);
            _material.Apply(
                device,
                Transform.WorldMatrix,
                camera.View,
                camera.Projection,
                _overrides,
                () => device.DrawIndexedPrimitives(_meshFilter.PrimitiveType, 0, 0, _meshFilter.PrimitiveCount));
        }
        #endregion

        #region Lifecycle Methods
        protected override void Start()
        {
            if (GameObject == null)
                return;

            _meshFilter = GameObject.GetComponent<MeshFilter>();
        }
        #endregion

        #region Housekeeping Methods
        #endregion
    }
}
