using MathNet.Spatial.Euclidean;


namespace CameraProjection.Math
{
    // Math.NET does not have a Line2D class
    public class Line2D
    {
        public Point2D Start { get; set; }
        public Point2D End { get; set; }
    }
}
