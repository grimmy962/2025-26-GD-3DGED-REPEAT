using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Factories
{
    /// <summary>
    /// Utility factory for building simple GPU meshes for quick demos and tests.
    /// Produces MeshFilter components ready to attach to a <see cref="GameObject"/>.
    /// </summary>
    /// <see cref="MeshFilter"/>
    /// <see cref="Entities.GameObject"/>
    public class MeshFilterFactory
    {
        #region Unlit

        #region Wireframe
        public static MeshFilter CreateMyFirstInitial(GraphicsDevice device)
        {
            //TODO - Homework
            throw new NotImplementedException("add initial (N)");
        }

        /// <summary>
        /// Creates XYZ axes as colored lines (X=Red, Y=Green, Z=Blue) starting at the origin.
        /// </summary>
        public static MeshFilter CreateAxesXYZ(GraphicsDevice device, float length = 1f)
        {
            var verts = new VertexPositionColor[6];
            // X axis (red)
            verts[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Red);
            verts[1] = new VertexPositionColor(new Vector3(length, 0, 0), Color.Red);
            // Y axis (green)
            verts[2] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Green);
            verts[3] = new VertexPositionColor(new Vector3(0, length, 0), Color.Green);
            // Z axis (blue)
            verts[4] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Blue);
            verts[5] = new VertexPositionColor(new Vector3(0, 0, length), Color.Blue);

            var indices = new short[] { 0, 1, 2, 3, 4, 5 };

            var mf = new MeshFilter();
            mf.SetGeometry(device, verts, indices, PrimitiveType.LineList);
            return mf;
        }

        /// <summary>
        /// Creates an XY plane grid (triangles), centered at origin.
        /// </summary>
        public static MeshFilter CreatePlaneGrid(GraphicsDevice device, int cols = 10, int rows = 10, float tileSize = 1f)
        {
            if (cols < 1) cols = 1;
            if (rows < 1) rows = 1;

            int vx = cols + 1;
            int vy = rows + 1;
            int vertexCount = vx * vy;
            int indexCount = cols * rows * 6;

            var verts = new VertexPositionColor[vertexCount];
            var indices = new short[indexCount];

            var hl = 0.5f;
            var w = cols * tileSize;
            var h = rows * tileSize;
            Vector2 origin = new Vector2(-w * hl, -h * hl);

            int k = 0;
            for (int y = 0; y < vy; y++)
                for (int x = 0; x < vx; x++)
                    verts[k++] = new VertexPositionColor(new Vector3(origin.X + x * tileSize, origin.Y + y * tileSize, 0f), Color.White);

            int t = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int i0 = y * vx + x;
                    int i1 = i0 + 1;
                    int i2 = i0 + vx;
                    int i3 = i2 + 1;

                    indices[t++] = (short)i0; indices[t++] = (short)i1; indices[t++] = (short)i3;
                    indices[t++] = (short)i0; indices[t++] = (short)i3; indices[t++] = (short)i2;
                }
            }

            var mf = new MeshFilter();
            mf.SetGeometry(device, verts, indices, PrimitiveType.TriangleList);
            return mf;
        }

        /// <summary>
        /// Creates an XY wire grid (LineList), centered at origin.
        /// </summary>
        public static MeshFilter CreateWireGrid(GraphicsDevice device, int cols = 10, int rows = 10, float spacing = 1f)
        {
            if (cols < 1)
                cols = 1;
            if (rows < 1)
                rows = 1;

            int verticalLines = cols + 1;
            int horizontalLines = rows + 1;

            var verts = new VertexPositionColor[(verticalLines + horizontalLines) * 2];

            var hl = 0.5f;
            var w = cols * spacing;
            var h = rows * spacing;
            var x0 = -w * hl;
            var y0 = -h * hl;

            int k = 0;
            // verticals
            for (int c = 0; c <= cols; c++)
            {
                float x = x0 + c * spacing;
                var col = c % 5 == 0 ? Color.LightGray : Color.White; // inline, simple variation
                verts[k++] = new VertexPositionColor(new Vector3(x, y0, 0f), col);
                verts[k++] = new VertexPositionColor(new Vector3(x, y0 + h, 0f), col);
            }
            // horizontals
            for (int r = 0; r <= rows; r++)
            {
                float y = y0 + r * spacing;
                var col = r % 5 == 0 ? Color.LightGray : Color.White; // inline, simple variation
                verts[k++] = new VertexPositionColor(new Vector3(x0, y, 0f), col);
                verts[k++] = new VertexPositionColor(new Vector3(x0 + w, y, 0f), col);
            }

            var indices = new short[verts.Length];
            for (short i = 0; i < indices.Length; i++) indices[i] = i;

            var mf = new MeshFilter();
            mf.SetGeometry(device, verts, indices, PrimitiveType.LineList);
            return mf;
        }

        /// <summary>
        /// Creates a wireframe axis-aligned box (LineList), centered at origin.
        /// </summary>
        public static MeshFilter CreateWireBox(GraphicsDevice device, Vector3? size = null)
        {
            var hl = 0.5f;
            Vector3 s = size ?? Vector3.One;
            Vector3 h = s * hl;

            var p = new[]
            {
                new Vector3(-h.X, -h.Y, -h.Z), new Vector3( h.X, -h.Y, -h.Z),
                new Vector3( h.X,  h.Y, -h.Z), new Vector3(-h.X,  h.Y, -h.Z),
                new Vector3(-h.X, -h.Y,  h.Z), new Vector3( h.X, -h.Y,  h.Z),
                new Vector3( h.X,  h.Y,  h.Z), new Vector3(-h.X,  h.Y,  h.Z),
            };

            var v = new VertexPositionColor[24];
            int k = 0;

            // bottom
            v[k++] = new VertexPositionColor(p[0], Color.White); v[k++] = new VertexPositionColor(p[1], Color.White);
            v[k++] = new VertexPositionColor(p[1], Color.White); v[k++] = new VertexPositionColor(p[2], Color.White);
            v[k++] = new VertexPositionColor(p[2], Color.White); v[k++] = new VertexPositionColor(p[3], Color.White);
            v[k++] = new VertexPositionColor(p[3], Color.White); v[k++] = new VertexPositionColor(p[0], Color.White);

            // top
            v[k++] = new VertexPositionColor(p[4], Color.White); v[k++] = new VertexPositionColor(p[5], Color.White);
            v[k++] = new VertexPositionColor(p[5], Color.White); v[k++] = new VertexPositionColor(p[6], Color.White);
            v[k++] = new VertexPositionColor(p[6], Color.White); v[k++] = new VertexPositionColor(p[7], Color.White);
            v[k++] = new VertexPositionColor(p[7], Color.White); v[k++] = new VertexPositionColor(p[4], Color.White);

            // verticals
            v[k++] = new VertexPositionColor(p[0], Color.White); v[k++] = new VertexPositionColor(p[4], Color.White);
            v[k++] = new VertexPositionColor(p[1], Color.White); v[k++] = new VertexPositionColor(p[5], Color.White);
            v[k++] = new VertexPositionColor(p[2], Color.White); v[k++] = new VertexPositionColor(p[6], Color.White);
            v[k++] = new VertexPositionColor(p[3], Color.White); v[k++] = new VertexPositionColor(p[7], Color.White);

            var indices = new short[v.Length];
            for (short i = 0; i < indices.Length; i++) indices[i] = i;

            var mf = new MeshFilter();
            mf.SetGeometry(device, v, indices, PrimitiveType.LineList);
            return mf;
        }

        #endregion

        #region Solid Colored
        /// <summary>
        /// Creates a colored triangle in the XY plane centered near the origin.
        /// </summary>
        public static MeshFilter CreateTriangleColored(GraphicsDevice device)
        {
            var hl = 0.5f;

            var verts = new VertexPositionColor[3];
            verts[0] = new VertexPositionColor(new Vector3(-hl, -hl, 0f), Color.Red);
            verts[1] = new VertexPositionColor(new Vector3(hl, -hl, 0f), Color.Green);
            verts[2] = new VertexPositionColor(new Vector3(0.0f, hl, 0f), Color.Blue);

            var indices = new short[] { 0, 1, 2 };

            var mf = new MeshFilter();
            mf.SetGeometry(device, verts, indices, PrimitiveType.TriangleList);
            return mf;
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
        /// Creates a solid (triangle) box centered at origin, aligned to axes.
        /// </summary>
        public static MeshFilter CreateBoxColored(GraphicsDevice device, Vector3? size = null)
        {
            Vector3 s = size ?? Vector3.One;
            Vector3 h = s * 0.5f;

            var v = new VertexPositionColor[24];

            // +Z (front)
            v[0] = new VertexPositionColor(new Vector3(-h.X, -h.Y, h.Z), Color.Red);
            v[1] = new VertexPositionColor(new Vector3(h.X, -h.Y, h.Z), Color.Red);
            v[2] = new VertexPositionColor(new Vector3(h.X, h.Y, h.Z), Color.Red);
            v[3] = new VertexPositionColor(new Vector3(-h.X, h.Y, h.Z), Color.Red);

            // -Z (back)
            v[4] = new VertexPositionColor(new Vector3(h.X, -h.Y, -h.Z), Color.Green);
            v[5] = new VertexPositionColor(new Vector3(-h.X, -h.Y, -h.Z), Color.Green);
            v[6] = new VertexPositionColor(new Vector3(-h.X, h.Y, -h.Z), Color.Green);
            v[7] = new VertexPositionColor(new Vector3(h.X, h.Y, -h.Z), Color.Green);

            // +X (right)
            v[8] = new VertexPositionColor(new Vector3(h.X, -h.Y, h.Z), Color.Blue);
            v[9] = new VertexPositionColor(new Vector3(h.X, -h.Y, -h.Z), Color.Blue);
            v[10] = new VertexPositionColor(new Vector3(h.X, h.Y, -h.Z), Color.Blue);
            v[11] = new VertexPositionColor(new Vector3(h.X, h.Y, h.Z), Color.Blue);

            // -X (left)
            v[12] = new VertexPositionColor(new Vector3(-h.X, -h.Y, -h.Z), Color.Yellow);
            v[13] = new VertexPositionColor(new Vector3(-h.X, -h.Y, h.Z), Color.Yellow);
            v[14] = new VertexPositionColor(new Vector3(-h.X, h.Y, h.Z), Color.Yellow);
            v[15] = new VertexPositionColor(new Vector3(-h.X, h.Y, -h.Z), Color.Yellow);

            // +Y (top)
            v[16] = new VertexPositionColor(new Vector3(-h.X, h.Y, h.Z), Color.Cyan);
            v[17] = new VertexPositionColor(new Vector3(h.X, h.Y, h.Z), Color.Cyan);
            v[18] = new VertexPositionColor(new Vector3(h.X, h.Y, -h.Z), Color.Cyan);
            v[19] = new VertexPositionColor(new Vector3(-h.X, h.Y, -h.Z), Color.Cyan);

            // -Y (bottom)
            v[20] = new VertexPositionColor(new Vector3(-h.X, -h.Y, -h.Z), Color.Magenta);
            v[21] = new VertexPositionColor(new Vector3(h.X, -h.Y, -h.Z), Color.Magenta);
            v[22] = new VertexPositionColor(new Vector3(h.X, -h.Y, h.Z), Color.Magenta);
            v[23] = new VertexPositionColor(new Vector3(-h.X, -h.Y, h.Z), Color.Magenta);

            short[] i =
            {
                0,1,2,  0,2,3,     // front
                4,5,6,  4,6,7,     // back
                8,9,10, 8,10,11,   // right
                12,13,14, 12,14,15,// left
                16,17,18, 16,18,19,// top
                20,21,22, 20,22,23 // bottom
            };

            var mf = new MeshFilter();
            mf.SetGeometry(device, v, i, PrimitiveType.TriangleList);
            return mf;
        }
        #endregion

        #region Solid Textured
        /// <summary>
        /// Creates a unit textured quad (1x1) on the XY plane centered at the origin.
        /// Unlit variant: POSITION + TEXCOORD only.
        /// </summary>
        public static MeshFilter CreateQuadTextured(GraphicsDevice device)
        {
            var halfLength = 0.5f;

            var verts = new VertexPositionTexture[4];
            // 0-BL, 1-BR, 2-TL, 3-TR  (matches CreateQuadColored)
            verts[0] = new VertexPositionTexture(new Vector3(-halfLength, -halfLength, 0f), new Vector2(0f, 1f)); // BL
            verts[1] = new VertexPositionTexture(new Vector3(halfLength, -halfLength, 0f), new Vector2(1f, 1f)); // BR
            verts[2] = new VertexPositionTexture(new Vector3(-halfLength, halfLength, 0f), new Vector2(0f, 0f)); // TL
            verts[3] = new VertexPositionTexture(new Vector3(halfLength, halfLength, 0f), new Vector2(1f, 0f)); // TR

            // Clockwise (left-hand) to match CreateQuadColored
            var indices = new short[] {
                            2, 1, 0,   // bottom tri
                            3, 1, 2    // top tri
                        };

            var mf = new MeshFilter();
            mf.SetGeometry(device, verts, indices, PrimitiveType.TriangleList);
            return mf;
        }

        #endregion

        #endregion

        #region Lit

        #region Solid Textured
        /// <summary>
        /// Creates a unit textured quad (1x1) on the XY plane centered at the origin,
        /// with normals pointing +Z for basic lighting (e.g., BasicEffect LightingEnabled=true).
        /// </summary>
        public static MeshFilter CreateQuadTexturedLit(GraphicsDevice device)
        {
            var hl = 0.5f;
            var n = new Vector3(0f, 0f, 1f);

            var verts = new VertexPositionNormalTexture[4];
            // 0-BL, 1-BR, 2-TL, 3-TR (same order as colored/textured)
            verts[0] = new VertexPositionNormalTexture(new Vector3(-hl, -hl, 0f), n, new Vector2(0f, 1f)); // BL
            verts[1] = new VertexPositionNormalTexture(new Vector3(hl, -hl, 0f), n, new Vector2(1f, 1f)); // BR
            verts[2] = new VertexPositionNormalTexture(new Vector3(-hl, hl, 0f), n, new Vector2(0f, 0f)); // TL
            verts[3] = new VertexPositionNormalTexture(new Vector3(hl, hl, 0f), n, new Vector2(1f, 0f)); // TR

            var indices = new short[] {
                            2, 1, 0,
                            3, 1, 2
                        };

            var mf = new MeshFilter();
            mf.SetGeometry(device, verts, indices, PrimitiveType.TriangleList);
            return mf;
        }

        #endregion

        #region Solid Colored
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
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (string.IsNullOrWhiteSpace(assetName))
                throw new ArgumentException("Asset name must be non-empty.", nameof(assetName));

            // Load model and select mesh/part
            var model = content.Load<Model>(assetName);
            if (meshIndex < 0 || meshIndex >= model.Meshes.Count)
                throw new ArgumentOutOfRangeException(nameof(meshIndex), "meshIndex outside range of Model.Meshes.");

            var mesh = model.Meshes[meshIndex];
            if (partIndex < 0 || partIndex >= mesh.MeshParts.Count)
                throw new ArgumentOutOfRangeException(nameof(partIndex), "partIndex outside range of ModelMesh.MeshParts.");

            var part = mesh.MeshParts[partIndex];

            // Vertex copy (slice the part's vertex range into a standalone buffer)
            var vertexDecl = part.VertexBuffer.VertexDeclaration;
            int vertexStride = vertexDecl.VertexStride;
            int vertexCount = part.NumVertices;
            int vertexOffsetBytes = part.VertexOffset * vertexStride;

            var vertexBytes = new byte[vertexStride * vertexCount];
            part.VertexBuffer.GetData(vertexOffsetBytes, vertexBytes, 0, vertexBytes.Length, vertexStride);

            var vb = new VertexBuffer(device, vertexDecl, vertexCount, BufferUsage.WriteOnly);
            vb.SetData(vertexBytes);

            // Index copy (slice indices for this part and rebase to zero because we won't use baseVertex)
            int indexCount = part.PrimitiveCount * 3;
            int indexStartByte = part.StartIndex * (part.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            IndexBuffer ib;
            if (part.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
            {
                var src = new ushort[indexCount];
                part.IndexBuffer.GetData(indexStartByte, src, 0, indexCount);

                // Rebase to local vertex 0
                short[] dst = new short[indexCount];
                for (int i = 0; i < indexCount; i++)
                    dst[i] = (short)(src[i] - part.VertexOffset);

                ib = new IndexBuffer(device, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);
                ib.SetData(dst);
            }
            else
            {
                var src = new int[indexCount];
                part.IndexBuffer.GetData(indexStartByte, src, 0, indexCount);

                // Rebase to local vertex 0
                for (int i = 0; i < indexCount; i++)
                    src[i] = src[i] - part.VertexOffset;

                ib = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indexCount, BufferUsage.WriteOnly);
                ib.SetData(src);
            }

            // Package into MeshFilter
            var mf = new MeshFilter();
            mf.SetGeometry(vb, ib, PrimitiveType.TriangleList, indexCount);
            return mf;
        }
        #endregion

        #endregion
    }
}
