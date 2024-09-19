using MagratheaCore.Utility;
using Microsoft.Xna.Framework;
using System;

namespace MagratheaCore.Environment
{
    public abstract class HeightProvider
    {
        #region Constants

        private const float NormalSampleOffset = 1;

        #endregion

        #region Methods

        public abstract float GetHeight(double x, double y, double z);

        public float GetHeight(Vector3Double location)
        {
            return GetHeight(location.X, location.Y, location.Z);
        }

        public Vector3 GetNormalFromFiniteOffset(Vector3Double location)
        {
            Vector3 normalisedLocation = Vector3Double.Normalize(location);
            Vector3 arbitraryUnitVector = Math.Abs(normalisedLocation.Y) > Math.Abs(normalisedLocation.X) ? Vector3.UnitX : Vector3.UnitY;
            Vector3 tangentVector1 = Vector3.Normalize(Vector3.Cross(arbitraryUnitVector, normalisedLocation));
            Vector3 tangentVector2 = Vector3.Normalize(Vector3.Cross(tangentVector1, normalisedLocation));

            float hL = GetHeight(location - tangentVector1*NormalSampleOffset);
            float hR = GetHeight(location + tangentVector1*NormalSampleOffset);
            float hD = GetHeight(location - tangentVector2*NormalSampleOffset);
            float hU = GetHeight(location + tangentVector2*NormalSampleOffset);

            return Vector3.Normalize(2*normalisedLocation + (hL - hR)*tangentVector1 + (hD - hU)*tangentVector2);
        }

        #endregion
    }
}