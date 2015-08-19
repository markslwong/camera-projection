using System.Windows.Media;
using System.Windows.Media.Media3D;
using MathNet.Spatial.Units;

using Point3D = MathNet.Spatial.Euclidean.Point3D;
using WindowsPoint3D = System.Windows.Media.Media3D.Point3D;
using WindowsVector3D = System.Windows.Media.Media3D.Vector3D;

namespace CameraProjection
{
    public partial class MainWindow
    {
        private const double RenderLineSize = 0.1;
        private const double CameraTriangleSize = 1;

        private static GeometryModel3D CreateCamera(Camera camera,  double size, Brush brush)
        {
            var mesh = new MeshGeometry3D();
            
            var position = camera.Position;
            var direction = camera.Direction;

            var angle = System.Math.Atan2(direction.Y, direction.X);

            const double equalaterialTriangleInternalAngle = 22.5; // 45 degrees in half

            var ratio = Angle.FromDegrees(equalaterialTriangleInternalAngle).Radians;

            var multiplier = size / (1 + ratio);

            mesh.Positions.Add(new WindowsPoint3D(-multiplier, 0, 0));
            mesh.Positions.Add(new WindowsPoint3D(+multiplier * ratio, -size * 0.5, 0));
            mesh.Positions.Add(new WindowsPoint3D(+multiplier * ratio, +size * 0.5, 0));

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            return new GeometryModel3D(mesh, new DiffuseMaterial(brush))
            {
                Transform = new Transform3DGroup
                {
                    Children = new Transform3DCollection(new Transform3D [] {
                        new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle / System.Math.PI * 180)),
                        new TranslateTransform3D(position.X, position.Y, position.Z)
                    })
                }
            };
        }

        private static GeometryModel3D CreateLine(Point3D start, Point3D end, double lineThickness, Brush brush)
        {
            var mesh = new MeshGeometry3D();

            var deltaX = end.X - start.X;
            var deltaY = end.Y - start.Y;
            
            var angle = System.Math.Atan2(deltaY, deltaX);

            const double halfPi = System.Math.PI * 0.5;
            var halfThickness = lineThickness * 0.5f;

            var lineX = System.Math.Cos(angle + halfPi) * halfThickness;
            var lineY = System.Math.Sin(angle + halfPi) * halfThickness;

            mesh.Positions.Add(new WindowsPoint3D(start.X + lineX, start.Y + lineY, 0));
            mesh.Positions.Add(new WindowsPoint3D(start.X - lineX, start.Y - lineY, 0));
            mesh.Positions.Add(new WindowsPoint3D(end.X - lineX, end.Y - lineY, 0));
            mesh.Positions.Add(new WindowsPoint3D(end.X + lineX, end.Y + lineY, 0));

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(0);

            return new GeometryModel3D(mesh, new DiffuseMaterial(brush));
        }

        private void RefreshViewport()
        {
            var projections = _viewModel.CameraProjections;

            ModelGroupCameras.Children.Clear(); // TODO: refresh individual items.  This is quite terrible.
            ModelGroupCameras.Children.Add(new AmbientLight(Colors.White));

            foreach (var projection in projections)
            {
                foreach (var line in projection.Lines)
                {
                    var renderLine = CreateLine(line.StartPoint, line.EndPoint, RenderLineSize, Brushes.White);
                    ModelGroupCameras.Children.Add(renderLine);
                }

                var renderCamera = CreateCamera(projection.Camera, CameraTriangleSize, Brushes.CornflowerBlue);
                ModelGroupCameras.Children.Add(renderCamera);
            }
        }
    }
}
