using MathNet.Spatial.Euclidean;

namespace CameraProjection
{
    public abstract class Entity
    {
        private readonly Point3D _position = new Point3D();

        public Point3D Position
        {
            get { return _position; }
        }
    }
}
