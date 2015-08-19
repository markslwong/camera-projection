﻿using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;


namespace CameraProjection
{
    public class Camera : Entity
    {
        public Camera()
        {
            FieldOfView = 60;
            AspectRatio = 1.333333f;
            Position = new Point3D(0, 0, 2.5);
        }

        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }

        public float FieldOfView { get; set; }
        public float AspectRatio { get; set; }

        public IList<Ray3D> ComputeProjectionRays()
        {
            var rays = new List<Ray3D>();
            
            var coordinateSystem = CoordinateSystem.Rotation(Angle.FromDegrees(Yaw), Angle.FromDegrees(Pitch), Angle.FromDegrees(Roll));
            var matrix = coordinateSystem.GetRotationSubMatrix();

            // Since our camera sits in the middle of our field of view we split the FOV by half.
            var addedYaw = FieldOfView * 0.5f;
            var addedPitch = addedYaw / AspectRatio;

            var coordinateSystem1 = CoordinateSystem.Rotation(Angle.FromDegrees(+addedYaw), Angle.FromDegrees(+addedPitch), Angle.FromDegrees(0));
            var coordinateSystem2 = CoordinateSystem.Rotation(Angle.FromDegrees(-addedYaw), Angle.FromDegrees(+addedPitch), Angle.FromDegrees(0));
            var coordinateSystem3 = CoordinateSystem.Rotation(Angle.FromDegrees(+addedYaw), Angle.FromDegrees(-addedPitch), Angle.FromDegrees(0));
            var coordinateSystem4 = CoordinateSystem.Rotation(Angle.FromDegrees(-addedYaw), Angle.FromDegrees(-addedPitch), Angle.FromDegrees(0));

            var matrix1 = coordinateSystem1.GetRotationSubMatrix();
            var matrix2 = coordinateSystem2.GetRotationSubMatrix();
            var matrix3 = coordinateSystem3.GetRotationSubMatrix();
            var matrix4 = coordinateSystem4.GetRotationSubMatrix();

            // TODO: Don't like object creation.  Get it working for now and fix this later.
            // TODO: I know this will suffer from gimbal lock.  However, the Quaternion libraries both my Microsoft and by Math.Net do not have Euler to Quaternion conversions.  I've done the conversion manually in the past, but running out of time.
            var vector1 = matrix * matrix1 * UnitVector3D.XAxis.ToVector();
            var vector2 = matrix * matrix2 * UnitVector3D.XAxis.ToVector();
            var vector3 = matrix * matrix3 * UnitVector3D.XAxis.ToVector();
            var vector4 = matrix * matrix4 * UnitVector3D.XAxis.ToVector();

            rays.Add(new Ray3D(Position, new Vector3D(vector1)));
            rays.Add(new Ray3D(Position, new Vector3D(vector2)));
            rays.Add(new Ray3D(Position, new Vector3D(vector3)));
            rays.Add(new Ray3D(Position, new Vector3D(vector4)));

            return rays;
        }
    }
}
