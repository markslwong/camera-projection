using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;


namespace CameraProjection
{
    // I know this will suffer from gimbal lock.  However, it does not make sense for our 
    public class Camera : Entity
    {
        public Camera()
        {
            FieldOfView = 100;
            AspectRatio = 1.333333f;
        }

        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }

        public float FieldOfView { get; set; }
        public float AspectRatio { get; set; }

        public IList<Ray3D> ComputeProjectionRays()
        {
            var rays = new List<Ray3D>();

            // Since our camera sits in the middle of our field of view we split the FOV by half.
            var addedYaw = FieldOfView * 0.5f;
            var addedPitch = addedYaw / AspectRatio;

            var roll = Angle.FromDegrees(Roll);

            var coordinateSystem1 = CoordinateSystem.Rotation(Angle.FromDegrees(Yaw + addedYaw), Angle.FromDegrees(Pitch + addedPitch), roll);
            var coordinateSystem2 = CoordinateSystem.Rotation(Angle.FromDegrees(Yaw - addedYaw), Angle.FromDegrees(Pitch + addedPitch), roll);
            var coordinateSystem3 = CoordinateSystem.Rotation(Angle.FromDegrees(Yaw - addedYaw), Angle.FromDegrees(Pitch - addedPitch), roll);
            var coordinateSystem4 = CoordinateSystem.Rotation(Angle.FromDegrees(Yaw + addedYaw), Angle.FromDegrees(Pitch - addedPitch), roll);
            
            var vector1 = coordinateSystem1.Transform(UnitVector3D.XAxis);
            var vector2 = coordinateSystem2.Transform(UnitVector3D.XAxis); 
            var vector3 = coordinateSystem3.Transform(UnitVector3D.XAxis); 
            var vector4 = coordinateSystem4.Transform(UnitVector3D.XAxis);

            rays.Add(new Ray3D(Position, vector1));
            rays.Add(new Ray3D(Position, vector2));
            rays.Add(new Ray3D(Position, vector3));
            rays.Add(new Ray3D(Position, vector4));

            return rays;
        }
    }
}
