using GDEngine.Core.Components;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Rendering
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
        public Texture2D _texture;

        #region Fields
        private BasicEffect _effect;
        private MeshFilter? _meshFilter;
        #endregion

        #region Properties
        //TODO - Wk5
        #endregion


        #region Core Methods
        /// <summary>
        /// Renders the assigned <see cref="MeshFilter"/> using the given <see cref="GraphicsDevice"/> and <see cref="Camera"/>.
        /// </summary>
        public void Render(GraphicsDevice device, Camera camera)
        {
            //exit if the Transform isnt set
            if (Transform == null)
                return;

            // make sure we have an effect (material)
            EnsureEffect(device);

            //w, v, p
            _effect.World = Transform.WorldMatrix;
            _effect.View = camera.View;
            _effect.Projection = camera.Projection;

            _effect.Texture = _texture;
            //lighting - no normals!

            //point GFX card to the vertex and index buffers
            device.SetVertexBuffer(_meshFilter?.VertexBuffer);
            device.Indices = _meshFilter?.IndexBuffer;

            //apply
            var passes = _effect.CurrentTechnique.Passes;
            for (int i = 0; i < passes.Count; i++)
            {
                //load w,v,p, and all lighting settings, and buffers
                passes[i].Apply();

                device.DrawIndexedPrimitives(_meshFilter.PrimitiveType,
                    0, 0, _meshFilter.PrimitiveCount);
            }
            //draw
        }

        /// <summary>
        /// Creates or recreates the internal <see cref="BasicEffect"/> for the current device.
        /// </summary>
        public void EnsureEffect(GraphicsDevice device)
        {
            if (_effect != null)
                return;

            _effect = new BasicEffect(device)
            {
                VertexColorEnabled = false,  //in texturedquad the verts[] have COLOR
                LightingEnabled = false,
                TextureEnabled = true
            };
        }
        #endregion

        #region Lifecycle Methods
        //TODO - Wk5 - Get MeshFilter
        protected override void Start()
        {
            _meshFilter = GameObject?.GetComponent<MeshFilter>();
        }


        #endregion

        #region Housekeeping Methods
        //TODO - Wk5 - Dispose
        #endregion
    }
}
