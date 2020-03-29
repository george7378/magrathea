using Microsoft.Xna.Framework;
using System;

namespace MagratheaCore.Utility
{
    /// <summary>
    /// Always at the world space origin - objects should be drawn relative to it
    /// </summary>
	public class CenteredCamera
	{
        #region Constants

        public const float NearPlaneDistance = 1;
        public const float FarPlaneDistance = 10000000;

        private const float UnscaledSpaceDistance = FarPlaneDistance/4;
        private const float ScaledSpaceDistance = FarPlaneDistance - UnscaledSpaceDistance;
        private const float InvisibleDistance = 100*FarPlaneDistance;

        #endregion

        #region Fields

        private Matrix _orientation;

        private Matrix _projectionMatrix;

        #endregion

        #region Properties

        /// <summary>
        /// Virtual world position - not included in the view matrix
        /// </summary>
        public Vector3Double Position { get; set; }

		public Matrix Orientation
        {
            get
            {
                return _orientation;
            }
            set
            {
                _orientation = value;

                Refresh();
            }
        }

        public Matrix ViewMatrix { get; private set; }

        public Matrix ProjectionMatrix
        {
            get
            {
                return _projectionMatrix;
            }
            set
            {
                _projectionMatrix = value;

                Refresh();
            }
        }

        public BoundingFrustum Frustum { get; private set; }

        public float MovementSpeed { get; set; }

        #endregion

        #region Private methods

        private void Refresh()
        {
            ViewMatrix = Matrix.CreateLookAt(Vector3.Zero, Orientation.Forward, Orientation.Up);
            Frustum = new BoundingFrustum(ViewMatrix*ProjectionMatrix);
        }

        #endregion

        #region Methods

        public Matrix GetWorldMatrix(Vector3Double targetPosition)
        {
            Matrix scaleMatrix, translationMatrix;

            Vector3Double targetRelativePosition = targetPosition - Position;
            double targetRelativeDistance = targetRelativePosition.Length();

            if (targetRelativeDistance > UnscaledSpaceDistance)
            {
                float scaledTargetRelativeDistance = (float)(UnscaledSpaceDistance + ScaledSpaceDistance*(1 - Math.Exp((ScaledSpaceDistance - targetRelativeDistance)/InvisibleDistance)));
                Vector3 scaledTargetRelativePosition = scaledTargetRelativeDistance*Vector3Double.Normalize(targetRelativePosition);

                scaleMatrix = Matrix.CreateScale((float)(scaledTargetRelativeDistance/targetRelativeDistance));
                translationMatrix = Matrix.CreateTranslation(scaledTargetRelativePosition);
            }
            else
            {
                scaleMatrix = Matrix.Identity;
                translationMatrix = Matrix.CreateTranslation(targetRelativePosition.ToVector3());
            }

            return scaleMatrix*translationMatrix;
        }

		#endregion
	}
}
