using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Rendering
{
    /// <summary>
    /// Handy render-state presets for students. Each returns a <see cref="RenderStates.RenderStateBlock"/>.
    /// </summary>
    public static class RenderStates
    {
        #region Static Fields
        #endregion

        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Methods
        /// <summary>
        /// Default opaque 3D: depth test/write, back-face cull, linear wrap sampling.
        /// </summary>
        public static RenderStateBlock Opaque3D()
        {
            return new RenderStateBlock(
                BlendState.Opaque,
                DepthStencilState.Default,
                RasterizerState.CullCounterClockwise,
                SamplerState.LinearWrap
            );
        }

        /// <summary>
        /// Transparent surfaces: alpha blend, depth read (no write), back-face cull, linear clamp.
        /// </summary>
        public static RenderStateBlock Transparent()
        {
            return new RenderStateBlock(
                BlendState.AlphaBlend,
                DepthStencilState.DepthRead,
                RasterizerState.CullCounterClockwise,
                SamplerState.LinearClamp
            );
        }

        /// <summary>
        /// Additive effects (glows, particles): additive blend, depth read, back-face cull, linear clamp.
        /// </summary>
        public static RenderStateBlock Additive()
        {
            return new RenderStateBlock(
                BlendState.Additive,
                DepthStencilState.DepthRead,
                RasterizerState.CullCounterClockwise,
                SamplerState.LinearClamp
            );
        }

        /// <summary>
        /// Wireframe debug: opaque blend, depth write, no culling, point-clamp sampling.
        /// </summary>
        public static RenderStateBlock Wireframe()
        {
            var raster = new RasterizerState
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None
            };

            return new RenderStateBlock(
                BlendState.Opaque,
                DepthStencilState.Default,
                raster,
                SamplerState.PointClamp
            );
        }

        /// <summary>
        /// Simple UI/sprite pass: alpha blend, no depth, no cull, linear clamp.
        /// </summary>
        public static RenderStateBlock UI2D()
        {
            return new RenderStateBlock(
                BlendState.AlphaBlend,
                DepthStencilState.None,
                RasterizerState.CullNone,
                SamplerState.LinearClamp
            );
        }
        #endregion

        #region Lifecycle Methods
        #endregion

        #region Housekeeping Methods
        /// <summary>
        /// Group of render states you can copy between materials.
        /// </summary>
        public readonly struct RenderStateBlock
        {
            public readonly BlendState? Blend;
            public readonly DepthStencilState? DepthStencil;
            public readonly RasterizerState? Rasterizer;
            public readonly SamplerState? Sampler;

            public RenderStateBlock(BlendState? blend, DepthStencilState? depthStencil, RasterizerState? rasterizer, SamplerState? sampler)
            {
                Blend = blend;
                DepthStencil = depthStencil;
                Rasterizer = rasterizer;
                Sampler = sampler;
            }
        }
        #endregion
    }
}
