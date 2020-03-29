using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MagratheaCore.Environment
{
    public class StarDome
    {
        #region Constants

        private const int NumberOfStars = 2000;

        private const float Radius = 10;

        #endregion

        #region Properties

        public List<Star> Stars { get; private set; }

        #endregion

        #region Constructors

        public StarDome(Random random)
        {
            CalculateContents(random);
        }

        #endregion

        #region Private methods

        private void CalculateContents(Random random)
		{
            Stars = new List<Star>();
			for (int i = 0; i < NumberOfStars; i++)
			{
                float altitudeParameter = (float)(2*random.NextDouble() - 1);
                float azimuthAngle = (float)(2*Math.PI*random.NextDouble());
                float azimuthMultiplier = (float)Math.Sqrt(1 - altitudeParameter*altitudeParameter);

				Vector3 domePosition = new Vector3((float)(azimuthMultiplier*Math.Sin(azimuthAngle)), altitudeParameter, (float)(-azimuthMultiplier*Math.Cos(azimuthAngle)))*Radius;
                float colourValue = (float)(0.1f + 0.4f*random.NextDouble());

                Star star = new Star(domePosition, new Color(colourValue, colourValue, colourValue));
                Stars.Add(star);
			}
		}

        #endregion
    }
}
