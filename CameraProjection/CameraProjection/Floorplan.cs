using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using CameraProjection.Math;
using MathNet.Spatial.Euclidean;


namespace CameraProjection
{
    public class Floorplan
    {
        private readonly List<Line2D> _walls = new List<Line2D>();

        private Plane _planeGround = new Plane(new Point3D(), UnitVector3D.ZAxis);

        private Size _size;
        private IList<Plane> _planes; 

        public void Clear()
        {
            _walls.Clear();
        }

        public void SetSize(Size size)
        {
            _size = size;

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
            var planes = new Plane?[rays.Count];
            
            for (var i = 0; i < rays.Count; ++i)
            {
                var ray = rays[i];
                var boundary = false;

                var shortestDistance = double.MaxValue;
                Point3D? shortestProjection = null;

                try
                {
                    var rayPlane = new Plane(ray.ThroughPoint, ray.Direction);
                    var projection = _planeGround.IntersectionWith(ray);
                    var distance = rayPlane.SignedDistanceTo(projection);

                    if (distance > 0)
                    {
                        shortestDistance = distance;
                        shortestProjection = projection;
                    }
                }
                catch
                {
                    // Exception is thrown if no intersection occurs.
                    // Normally I would strongly have this exception catch strongly typed 
                    // so it doesn't surpress other exceptions.  However, the documentation 
                    // for Math.NET does not list which exception will be thrown.  It's a bit
                    // dangerous to sift through the source since you're have no gaurantee
                    // that it will work across versions, so it's best to tighten our scope.
                }

                if (!shortestProjection.HasValue)
                {
                    boundary = true;
                }

                foreach (var plane in _planes)
                {
                    try
                    {
                        var rayPlane = new Plane(ray.ThroughPoint, ray.Direction);
                        var projection = plane.IntersectionWith(ray);
                        var distance = rayPlane.SignedDistanceTo(projection);

                        if (distance > 0 &&
                            shortestDistance > distance)
                        {
                            shortestDistance = distance;
                            shortestProjection = projection;
                            planes[i] = plane;
                        }
                    }
                    catch
                    {
                        // See comment for try-catch above.
                    }
                }

                corners[i] = shortestProjection;

                if (i == rays.Count - 1)
                {
                    if (corners[0].HasValue)
                    {
                        if (boundary)
                        {
                            AddProjectionsAtBoundary(projections, camera.Position, corners[0].Value, shortestProjection.Value);
                        }
                        else
                        {
                            projections.Add(new Line3D(corners[0].Value, shortestProjection.Value)); 
                        }
                    }
                }

                if (i > 0 && corners[i - 1].HasValue)
                {
                    if (boundary)
                    {
                        AddProjectionsAtBoundary(projections, camera.Position, corners[i - 1].Value, shortestProjection.Value);
                    }
                    else
                    {
                        projections.Add(new Line3D(corners[i - 1].Value, shortestProjection.Value)); 
                    }
                }
            }

            return projections;
        }

        private class ProjectionAnalysis
        {
            public ProjectionAnalysis(Point3D point, Point3D origin)
            {
                Vector = new Vector2D(point.X - origin.X, point.Y - origin.Y);
                Vector = Vector.Normalize();
                Point = point;
            }

            public ProjectionAnalysis(double x, double y, Point3D origin)
                : this(new Point3D(x, y, 0), origin)
            {}

            private Vector2D Vector { get; set; }
            public Point3D Point { get; private set; }

            public double Radians
            {
                get { return System.Math.Atan2(Vector.Y, Vector.X) + System.Math.PI; }
            }
        }

        private void AddProjectionsAtBoundary(ICollection<Line3D> projections, Point3D camera, Point3D p1, Point3D p2)
        {
            var analysis = new List<ProjectionAnalysis>();

            ProjectionAnalysis a1;
            ProjectionAnalysis a2;

            try
            {
                a1 = new ProjectionAnalysis(p1, camera);
                a2 = new ProjectionAnalysis(p2, camera);

                analysis.Add(a1);
                analysis.Add(a2);

                // Swap order so that the smallest angle is first
                if (a1.Radians > a2.Radians)
                {
                    var temp = a2;
                    a2 = a1;
                    a1 = temp;
                }

                var halfWidth = _size.Width * 0.5;
                var halfHeight = _size.Height * 0.5;

                analysis.Add(new ProjectionAnalysis(halfWidth, halfHeight, camera));
                analysis.Add(new ProjectionAnalysis(-halfWidth, halfHeight, camera));
                analysis.Add(new ProjectionAnalysis(halfWidth, -halfHeight, camera));
                analysis.Add(new ProjectionAnalysis(-halfWidth, -halfHeight, camera));
            }
            catch (Exception)
            {
                return;
            }

            var ordered = analysis
                .OrderBy(x => x.Radians)
                .Where(x => x.Radians >= a1.Radians && x.Radians <= a2.Radians)
                .ToArray();

            for (var i = 1; i < ordered.Length; ++i)
            {
                projections.Add(new Line3D(ordered[i - 1].Point, ordered[i].Point)); 
            }
        }
    }
}
