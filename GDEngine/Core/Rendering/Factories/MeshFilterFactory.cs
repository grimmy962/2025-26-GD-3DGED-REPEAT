using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Rendering.Factories
{
    /// <summary>
    /// Utility factory for building simple GPU meshes for quick demos and tests.
    /// Produces MeshFilter components ready to attach to a <see cref="GameObject"/>.
    /// </summary>
    /// <see cref="MeshFilter"/>
    /// <see cref="GameObject"/>
    public class MeshFilterFactory
    {
        /// <summary>
        /// Creates XYZ axes as colored lines (X=Red, Y=Green, Z=Blue) starting at the origin.
        /// </summary>
        public static MeshFilter CreateAxesXYZ(GraphicsDevice device, float length = 1f)
        {
            //TODO - Wk 5
            throw new NotImplementedException();
        }
        /// <summary>
        /// Creates a colored triangle in the XY plane centered near the origin.
        /// </summary>
        public static MeshFilter CreateTriangleColored(GraphicsDevice device)
        {
            //TODO - Wk 5
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a unit quad (1x1) in the XY plane centered at the origin, with per-vertex color.
        /// </summary>
        public static MeshFilter CreateQuadColored(GraphicsDevice device)
        {
            var hl = 0.5f;

            //TODO - Wk 5
            var verts = new VertexPositionColor[4];
            verts[0] = new VertexPositionColor(
                new Vector3(-hl, -hl, 0), Color.Red); //0 - BL
            verts[1] = new VertexPositionColor(
               new Vector3(hl, -hl, 0), Color.Green);  //1- BR
            verts[2] = new VertexPositionColor(
               new Vector3(-hl, hl, 0), Color.Blue);  //2 - TL
            verts[3] = new VertexPositionColor(
               new Vector3(hl, hl, 0), Color.Yellow);  //3 - TR

            var indices = new short[] { 
                2, 1, 0, //bottom triangle - winding order (LEFT HAND) clockwise
                3, 1, 2  //top triangle  - winding order (LEFT HAND) clockwise
            };

            var meshFilter = new MeshFilter();
            meshFilter.SetGeometry(device, verts, indices, 
                PrimitiveType.TriangleList);
            return meshFilter;
        }


        /// <summary>
        /// Loads a compiled FBX content asset (MonoGame <see cref="Model"/>) and extracts one mesh part into a new <see cref="MeshFilter"/>.
        /// Copies vertex and index data into fresh GPU buffers owned by the returned <see cref="MeshFilter"/>.
        /// </summary>
        /// <param name="content">Content manager used to load the compiled model (e.g., "Models/Cube").</param>
        /// <param name="device">Graphics device for buffer creation.</param>
        /// <param name="assetName">Content pipeline asset name (without extension).</param>
        /// <param name="meshIndex">Which ModelMesh to use (default 0).</param>
        /// <param name="partIndex">Which ModelMeshPart to use within the mesh (default 0).</param>
        public static MeshFilter CreateFromModel(ContentManager content,
                                                 GraphicsDevice device,
                                                 string assetName,
                                                 int meshIndex = 0,
                                                 int partIndex = 0)
        {
            //TODO - Wk 5
            throw new NotImplementedException();
        }
    }
}
