using System;

namespace MagratheaCore.Utility
{
    public class NoiseProvider
    {
        #region Properties

        public int Seed { get; set; }

        public int NumberOfOctaves { get; set; }

        public float Persistence { get; set; }

        public float Zoom { get; set; }

        #endregion

        #region Constructors

        public NoiseProvider(int seed, int numberOfOctaves, float persistence, float zoom)
        {
            Seed = seed;
            NumberOfOctaves = numberOfOctaves;
            Persistence = persistence;
            Zoom = zoom;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns a value between 0 and 1
        /// </summary>
        private float GridNoise(int x, int y, int z)
        {
            int n = (1619*x + 31337*y + 6971*z + 1013*Seed) & 0x7fffffff;
            n = (n >> 13) ^ n;

            return ((n*(n*n*60493 + 19990303) + 1376312589) & 0x7fffffff)/(float)int.MaxValue;
        }

        private float InterpolatedGridNoise(double x, double y, double z)
        {
            int integerX = (int)x;
            float fractionalX = (float)(x - integerX);

            int integerY = (int)y;
            float fractionalY = (float)(y - integerY);

            int integerZ = (int)z;
            float fractionalZ = (float)(z - integerZ);

            float v1 = GridNoise(integerX, integerY, integerZ);
            float v2 = GridNoise(integerX + 1, integerY, integerZ);
            float v3 = GridNoise(integerX, integerY, integerZ + 1);
            float v4 = GridNoise(integerX + 1, integerY, integerZ + 1);
            float v5 = GridNoise(integerX, integerY + 1, integerZ);
            float v6 = GridNoise(integerX + 1, integerY + 1, integerZ);
            float v7 = GridNoise(integerX, integerY + 1, integerZ + 1);
            float v8 = GridNoise(integerX + 1, integerY + 1, integerZ + 1);

            float i1 = Globals.CosineInterpolate(v1, v2, fractionalX);
            float i2 = Globals.CosineInterpolate(v3, v4, fractionalX);
            float i3 = Globals.CosineInterpolate(v5, v6, fractionalX);
            float i4 = Globals.CosineInterpolate(v7, v8, fractionalX);
            float i5 = Globals.CosineInterpolate(i1, i2, fractionalZ);
            float i6 = Globals.CosineInterpolate(i3, i4, fractionalZ);

            return Globals.CosineInterpolate(i5, i6, fractionalY);
        }

        #endregion

        #region Public methods

        public float GetValue(double x, double y, double z)
        {
            float total = 0;
            for (int o = 0; o < NumberOfOctaves; o++)
            {
                int frequency = (int)Math.Pow(2, o);
                float amplitude = (float)Math.Pow(Persistence, o);

                total += InterpolatedGridNoise(Math.Abs(x)*frequency/Zoom, Math.Abs(y)*frequency/Zoom, Math.Abs(z)*frequency/Zoom)*amplitude;
            }

            return total;
        }

        #endregion
    }
}