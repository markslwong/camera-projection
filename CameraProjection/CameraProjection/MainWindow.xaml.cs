using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MathNet.Spatial.Units;

using Point3D = MathNet.Spatial.Euclidean.Point3D;
using WindowsPoint3D = System.Windows.Media.Media3D.Point3D;

namespace CameraProjection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Floorplan _floorplan = new Floorplan();

        public MainWindow()
        {
            InitializeComponent();

            var camera = new Camera();

            CreateFloorplan(5, 5);

            var preview = _floorplan.ComputeProjectionPreview(camera);

            foreach (var item in preview)
            {
                var line = CreateLine(item.StartPoint, item.EndPoint, 0.01f);
                ModelGroupCameras.Children.Add(line);
            }
        }

        private void CreateFloorplan(float width, float height)
        {
            var aspectRatioViewport = 900.0 / 720.0; // TODO: Hard coded values.  Compute this from viewport.
            var aspectRatioFloorplan = width / height;

            var floorplanWidth = width;
            var floorplanHeight = height;

            if (aspectRatioViewport > aspectRatioFloorplan)
            {
                floorplanWidth = (float)aspectRatioViewport * height;
            }
            else
            {
                floorplanHeight = width / (float)aspectRatioViewport;
            }

            var cameraFov = Angle.FromDegrees(SceneCamera.FieldOfView * 0.5f);
            var cameraDistance = (floorplanWidth * 0.5f) / Math.Tan(cameraFov.Radians);

            SceneCamera.Position = new WindowsPoint3D(0, 0, cameraDistance);

            _floorplan.Clear();
            _floorplan.SetSize(floorplanWidth, floorplanHeight);
        }

        private static GeometryModel3D CreateLine(Point3D start, Point3D end, double lineThickness)
        {
            var mesh = new MeshGeometry3D();

            var deltaX = end.X - start.X;
            var deltaY = end.Y - start.Y;

            var angle = Math.Atan2(deltaY, deltaX);

            const double halfPi = Math.PI * 0.5;
            var halfThickness = lineThickness * 0.5f;

            var lineX = Math.Cos(angle + halfPi) * halfThickness;
            var lineY = Math.Sin(angle + halfPi) * halfThickness;

            mesh.Positions.Add(new WindowsPoint3D(start.X + lineX, start.Y + lineY, start.Z));
            mesh.Positions.Add(new WindowsPoint3D(start.X - lineX, start.Y - lineY, end.Z));
            mesh.Positions.Add(new WindowsPoint3D(end.X - lineX, end.Y - lineY, end.Z));
            mesh.Positions.Add(new WindowsPoint3D(end.X + lineX, end.Y + lineY, end.Z));

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(0);

            return new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.White));
        }
    }
}
