using System.Collections.Generic;
using MathNet.Spatial.Euclidean;


namespace CameraProjection
{
    public class Projection
    {
        public Camera Camera { get; set; }
        public IEnumerable<Line3D> Lines { get; set; }
    }
}
