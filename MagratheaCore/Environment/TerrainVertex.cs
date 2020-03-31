using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagratheaCore.Environment
{
    public struct TerrainVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color BaseColour;

        public TerrainVertex(Vector3 position, Vector3 normal, Color baseColour)
        {
            Position = position;
            Normal = normal;

            BaseColour = baseColour;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float)*6, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
    }
}