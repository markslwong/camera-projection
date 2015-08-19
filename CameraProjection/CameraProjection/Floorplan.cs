using System;
using System.Collections.Generic;
using System.Windows;
using CameraProjection.Math;
using MathNet.Spatial.Euclidean;


namespace CameraProjection
{
    public class Floorplan
    {
        private readonly List<Line2D> _walls = new List<Line2D>();

        private Plane _planeGround = new Plane(new Point3D(), UnitVector3D.ZAxis);

        private IList<Plane> _planes; 

        public void Clear()
        {
            _walls.Clear();
        }

        public void SetSize(Size size)
        {
            var halfWidth = size.Width * 0.5;
            var halfHeight = size.Height * 0.5;

            _planes = new List<Plane>();

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
            _walls.Add(new Line2D
            {
                Start = start,
                End = end
            });
        }

        public IEnumerable<Line3D> ComputeFloorProjection(Camera camera)
        {
            var projections = new List<Line3D>();

            var rays = camera.ComputeProjectionRays();
            var corners = new Point3D?[rays.Count];
            
            for (var i = 0; i < rays.Count; ++i)
            {
                var ray = rays[i];

                var shortestDistance = double.MaxValue;
                Point3D? shortestProjection = null;

                try
                {
                    var projection = _planeGround.IntersectionWith(ray);
                    var distance = projection.DistanceTo(ray.ThroughPoint);

                    if (distance > 0)
                    {
                        shortestDistance = distance;
                        shortestProjection = projection;
                    }
                }
                catch (Exception)
                {
                    // Exception is thrown if no intersection occurs.
                    // Normally I would strongly have this exception catch strongly typed 
                    // so it doesn't surpress other exceptions.  However, the documentation 
                    // for Math.NET does not list which exception will be thrown.  It's a bit
                    // dangerous to sift through the source since you're have no gaurantee
                    // that it will work across versions, so it's best to tighten our scope.
                    continue;
                }

                if (!shortestProjection.HasValue)
                {
                    continue;
                }

                foreach (var plane in _planes)
                {
                    try
                    {
                        var projection = plane.IntersectionWith(ray);
                        var distance = projection.DistanceTo(ray.ThroughPoint);

                        if (distance > 0 &&
                            shortestDistance > distance)
                        {
                            shortestDistance = distance;
                            shortestProjection = projection;
                        }
                    }
                    catch
                    {
                        // See comment for try-catch above.
                    }
                }

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

            return projections;
        }
    }
}
