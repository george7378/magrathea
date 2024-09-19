using Microsoft.Xna.Framework;

namespace MagratheaCore.Environment
{
    public class Star
    {
        #region Properties

        public Vector3 DomePosition { get; set; }

        public Color Colour { get; set; }

        #endregion

        #region Constructors

        public Star(Vector3 domePosition, Color colour)
        {
            DomePosition = domePosition;
            Colour = colour;
        }

        #endregion
    }
}
