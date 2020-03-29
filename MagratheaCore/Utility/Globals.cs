using Microsoft.Xna.Framework;
using System;

namespace MagratheaCore.Utility
{
    public static class Globals
    {
        #region Standalone methods

        public static float Clamp(float x, float vLower, float vUpper)
        {
            return x < vLower ? vLower : x > vUpper ? vUpper : x;
        }

        public static float LinearInterpolate(float v1, float v2, float w)
        {
            float weight = Clamp(w, 0, 1);

            return v1 + (v2 - v1)*weight;
        }

        public static float CosineInterpolate(float v1, float v2, float w)
        {
            float weight = Clamp(w, 0, 1);

            return v1 + (v2 - v1)*(1 - (float)Math.Cos(weight*Math.PI))/2;
        }

        public static Matrix OrthonormaliseMatrix(Matrix matrix)
        {
            Vector3 up = Vector3.Normalize(matrix.Up);
            Vector3 forward = Vector3.Normalize(Vector3.Cross(up, matrix.Right));
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, up));

            Matrix result = Matrix.Identity;
            result.Up = up;
            result.Forward = forward;
            result.Right = right;

            return result;
        }

        #endregion
    }
}
