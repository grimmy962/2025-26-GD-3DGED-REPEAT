using GDEngine.Core.Components;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core
{
    /// <summary>
    /// Holds mesh geometry buffers (vertex/index) and draw topology for a <see cref="GameObject"/>.
    /// Attach alongside a MeshRenderer to render (e.g., with <see cref="BasicEffect"/>).
    /// </summary>
    /// <see cref="MeshRenderer"/>
    /// <see cref="GameObject"/>
    public sealed class MeshFilter : Component 
    {
        #region Fields
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private PrimitiveType _primitiveType;
        private int _primitiveCount;
        private int _vertexCount;
        private int _indexCount;
        #endregion

        #region Properties
        public VertexBuffer VertexBuffer => _vertexBuffer;
        public IndexBuffer IndexBuffer => _indexBuffer;
        public PrimitiveType PrimitiveType => _primitiveType;
        public int PrimitiveCount => _primitiveCount;
        public int VertexCount => _vertexCount;
        public int IndexCount => _indexCount;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an empty <see cref="MeshFilter"/>. Call one of the SetGeometry methods to provide buffers.
        /// </summary>
        public MeshFilter() 
        { 
        }
        #endregion

        #region Core Methods
        /// <summary>
        /// Assigns vertex/index buffers from raw arrays (generic vertex type).
        /// </summary>
        public void SetGeometry<T>(GraphicsDevice device,
                                   T[] vertices,
                                   short[] indices,
                                   PrimitiveType primitiveType)
            where T : struct, IVertexType
        {
            //TODO - Wk5 
        }

        /// <summary>
        /// Assigns pre-built buffers to this filter.
        /// </summary>
        public void SetGeometry(VertexBuffer vb, IndexBuffer ib, PrimitiveType primitiveType, int indexCount)
        {
            //TODO - Wk5 
        }

        private static int CalculatePrimitiveCount(int indexCount, PrimitiveType type)
        {
            if (type == PrimitiveType.TriangleList)
                return indexCount / 3;
            if (type == PrimitiveType.TriangleStrip)
                return indexCount - 2 < 0 ? 0 : indexCount - 2;
            if (type == PrimitiveType.LineList)
                return indexCount / 2;
            if (type == PrimitiveType.LineStrip)
                return indexCount - 1 < 0 ? 0 : indexCount - 1;
            return 0;
        }

        #endregion

        #region Housekeeping Methods
        //TODO - Wk5 - Dispose
        #endregion
    }
}
