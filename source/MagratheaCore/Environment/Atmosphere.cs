using MagratheaCore.Environment.Enums;
using Microsoft.Xna.Framework;

namespace MagratheaCore.Environment
{
    public class Atmosphere
    {
        #region Properties

        public AtmosphereRenderMode RenderMode { get; set; }

        public float Depth { get; set; }

        public Color Colour { get; set; }

        #endregion

        #region Constructors

        public Atmosphere(float depth, Color colour)
        {
            Depth = depth;
            Colour = colour;
        }

        #endregion
    }
}
