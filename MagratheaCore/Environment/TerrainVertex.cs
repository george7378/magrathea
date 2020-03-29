using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagratheaCore.Environment
{
    public struct TerrainVertex
    {
        public Vector3 Position;
        public Vector3 Normal;

        public TerrainVertex(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    }
}