using System.Collections.Generic;
using MathNet.Spatial.Euclidean;


namespace CameraProjection
{
    public class Floorplan
    {
        private readonly List<LineSegment2D> _walls = new List<LineSegment2D>();

        private readonly Plane _planeGround = new Plane(new Point3D(), UnitVector3D.ZAxis);

        private IList<Plane> _planes; 

        public void Clear()
        {
            _walls.Clear();
        }

        public void SetSize(float width, float height)
        {
            var halfWidth = width * 0.5;
            var halfHeight = height * 0.5;

            _planes = new List<Plane>
            {
                _planeGround
            };

            var top = new Plane(new Point3D(0, halfHeight, 0), UnitVector3D.YAxis);
            var bottom = new Plane(new Point3D(0, -halfHeight, 0), UnitVector3D.YAxis);
            var left = new Plane(new Point3D(-halfWidth, 0, 0), UnitVector3D.XAxis);
            var right = new Plane(new Point3D(halfWidth, 0, 0), UnitVector3D.XAxis);

            _planes.Add(top);
            _planes.Add(bottom);
            _planes.Add(left);
            _planes.Add(right);
        }

        public void AddWall(Point2D start, Point2D end)
        {
            _walls.Add(new LineSegment2D
            {
                Start = start,
                End = end
            });
        }

        public IEnumerable<Line3D> ComputeProjectionPreview(Camera camera)
        {
            var projections = new List<Line3D>();

            var rays = camera.ComputeProjectionRays();
            var corners = new Point3D?[rays.Count];
            
            for (var i = 0; i < rays.Count; ++i)
            {
                var ray = rays[i];

                var shortestDistance = double.MaxValue;
                Point3D? shortestProjection = null;
                
                foreach (var plane in _planes)
                {
                    try
                    {
                        var projection = plane.IntersectionWith(ray);
                        var distance = projection.DistanceTo(ray.ThroughPoint);

                        if (shortestDistance > distance &&
                            distance > 0)
                        {
                            shortestDistance = distance;
                            shortestProjection = projection;
                        }
                    }
                    catch
                    {
                        // Exception is thrown if no intersection occurs.
                        // I would normally strongly type on this exception so it doesn't surpress everything.
                        // However, the documentation does not list which exception occurs.
                    }
                }

                if (shortestProjection.HasValue)
                {
                    // Save point before calculating walls
                    corners[i] = shortestProjection;

                    if (i == rays.Count - 1)
                    {
                        if (corners[0].HasValue)
                        {
                            projections.Add(new Line3D(corners[0].Value, shortestProjection.Value));
                        }
                    }

                    if (i > 0 && corners[i - 1].HasValue)
                    {
                        projections.Add(new Line3D(corners[i - 1].Value, shortestProjection.Value));
                    }
                }

                // TODO: compute walls

                if (shortestProjection.HasValue)
                {
                    projections.Add(new Line3D(ray.ThroughPoint, shortestProjection.Value));
                }
            }

            return projections;
        }
    }
}
