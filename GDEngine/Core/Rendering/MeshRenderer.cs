// MeshRenderer.cs
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GDEngine.Core
{
    /// <summary>
    /// Minimal renderer that draws a <see cref="MeshFilter"/> with an unlit <see cref="BasicEffect"/>.
    /// Suitable for rendering primitives from <see cref="PrimitiveFactory"/>.
    /// </summary>
    /// <see cref="MeshFilter"/>
    /// <see cref="PrimitiveFactory"/>
    /// <see cref="Camera"/>
    public sealed class MeshRenderer : Component
    {
        #region Fields
        //TODO - Wk5
        #endregion

        #region Properties
        //TODO - Wk5
        #endregion

        #region Constructors
        public MeshRenderer() { }
        #endregion

        #region Core Methods
        /// <summary>
        /// Renders the assigned <see cref="MeshFilter"/> using the given <see cref="GraphicsDevice"/> and <see cref="Camera"/>.
        /// </summary>
        public void Render(GraphicsDevice device, Camera camera)
        {
            //TODO - Wk5
        }

        /// <summary>
        /// Creates or recreates the internal <see cref="BasicEffect"/> for the current device.
        /// </summary>
        public void EnsureEffect(GraphicsDevice device)
        {
            //TODO - Wk5
        }
        #endregion

        #region Lifecycle Methods
        //TODO - Wk5 - Get MeshFilter
        #endregion

        #region Housekeeping Methods
        //TODO - Wk5 - Dispose
        #endregion
    }
}
