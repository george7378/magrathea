using Microsoft.Xna.Framework;
using System;

namespace MagratheaCore.Utility
{
    public struct Vector3Double
    {
        #region Fields

        private static Vector3Double _zero = new Vector3Double(0, 0, 0);
        private static Vector3Double _unitX = new Vector3Double(1, 0, 0);
        private static Vector3Double _unitY = new Vector3Double(0, 1, 0);
        private static Vector3Double _unitZ = new Vector3Double(0, 0, 1);

        #endregion

        #region Properties

        public static Vector3Double Zero
        {
            get
            {
                return _zero;
            }
        }

        public static Vector3Double UnitX
        {
            get
            {
                return _unitX;
            }
        }

        public static Vector3Double UnitY
        {
            get
            {
                return _unitY;
            }
        }

        public static Vector3Double UnitZ
        {
            get
            {
                return _unitZ;
            }
        }

        #endregion

        #region Members

        public double X, Y, Z;

        #endregion

        #region Constructors

        public Vector3Double(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3Double(Vector3 vector) : this(vector.X, vector.Y, vector.Z)
        {
            
        }

        #endregion

        #region Methods

        public static double DistanceSquared(Vector3Double value1, Vector3Double value2)
        {
            double differenceX = value1.X - value2.X;
            double differenceY = value1.Y - value2.Y;
            double differenceZ = value1.Z - value2.Z;

            return differenceX*differenceX + differenceY*differenceY + differenceZ*differenceZ;
        }

        public static double Distance(Vector3Double value1, Vector3Double value2)
        {
            return Math.Sqrt(DistanceSquared(value1, value2));
        }

        public static Vector3 Normalize(Vector3Double value)
        {
            return (value/value.Length()).ToVector3();
        }

        public double Length()
        {
            return Distance(this, Zero);
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        #endregion

        #region Operators

        #region Pure

        public static Vector3Double operator +(Vector3Double value1, Vector3Double value2)
        {
            return new Vector3Double(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);
        }

        public static Vector3Double operator -(Vector3Double value)
        {
            return new Vector3Double(-value.X, -value.Y, -value.Z);
        }

        public static Vector3Double operator -(Vector3Double value1, Vector3Double value2)
        {
            return new Vector3Double(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);
        }

        public static Vector3Double operator *(double scaleFactor, Vector3Double value)
        {
            return new Vector3Double(value.X*scaleFactor, value.Y*scaleFactor, value.Z*scaleFactor);
        }

        public static Vector3Double operator *(Vector3Double value, double scaleFactor)
        {
            return new Vector3Double(value.X*scaleFactor, value.Y*scaleFactor, value.Z*scaleFactor);
        }

        public static Vector3Double operator *(Vector3Double value1, Vector3Double value2)
        {
            return new Vector3Double(value1.X*value2.X, value1.Y*value2.Y, value1.Z*value2.Z);
        }

        public static Vector3Double operator /(Vector3Double value1, Vector3Double value2)
        {
            return new Vector3Double(value1.X/value2.X, value1.Y/value2.Y, value1.Z/value2.Z);
        }

        public static Vector3Double operator /(Vector3Double value, double divider)
        {
            double factor = 1/divider;

            return new Vector3Double(value.X*factor, value.Y*factor, value.Z*factor);
        }

        /*
        public static bool operator ==(Vector3Double value1, Vector3Double value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z;
        }

        public static bool operator !=(Vector3Double value1, Vector3Double value2)
        {
            return !(value1 == value2);
        }
        */

        #endregion

        #region Mixed (float)

        public static Vector3Double operator *(float scaleFactor, Vector3Double value)
        {
            return new Vector3Double(value.X*scaleFactor, value.Y*scaleFactor, value.Z*scaleFactor);
        }

        public static Vector3Double operator *(Vector3Double value, float scaleFactor)
        {
            return new Vector3Double(value.X*scaleFactor, value.Y*scaleFactor, value.Z*scaleFactor);
        }

        public static Vector3Double operator /(Vector3Double value, float divider)
        {
            float factor = 1/divider;

            return new Vector3Double(value.X*factor, value.Y*factor, value.Z*factor);
        }

        #endregion

        #region Mixed (Vector3)

        public static Vector3Double operator +(Vector3Double value1, Vector3 value2)
        {
            return new Vector3Double(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);
        }

        public static Vector3Double operator +(Vector3 value1, Vector3Double value2)
        {
            return new Vector3Double(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);
        }

        public static Vector3Double operator -(Vector3Double value1, Vector3 value2)
        {
            return new Vector3Double(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);
        }

        public static Vector3Double operator -(Vector3 value1, Vector3Double value2)
        {
            return new Vector3Double(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);
        }

        public static Vector3Double operator *(Vector3Double value1, Vector3 value2)
        {
            return new Vector3Double(value1.X*value2.X, value1.Y*value2.Y, value1.Z*value2.Z);
        }

        public static Vector3Double operator *(Vector3 value1, Vector3Double value2)
        {
            return new Vector3Double(value1.X*value2.X, value1.Y*value2.Y, value1.Z*value2.Z);
        }

        public static Vector3Double operator /(Vector3Double value1, Vector3 value2)
        {
            return new Vector3Double(value1.X/value2.X, value1.Y/value2.Y, value1.Z/value2.Z);
        }

        public static Vector3Double operator /(Vector3 value1, Vector3Double value2)
        {
            return new Vector3Double(value1.X/value2.X, value1.Y/value2.Y, value1.Z/value2.Z);
        }

        #endregion

        #endregion
    }
}
