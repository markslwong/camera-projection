using System;
using System.Windows;
using MathNet.Spatial.Units;

using WindowsPoint3D = System.Windows.Media.Media3D.Point3D;


namespace CameraProjection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel = new MainWindowViewModel();
        
        private const int DesiredFloorplanSize = 50; // meters

        public MainWindow()
        {
            InitializeComponent();
            
            CameraProperties.DataContext = _viewModel;

            ButtonAdd.Click += OnButtonAddClick;

            _viewModel.PropertyChanged += OnPropertyChanged;

            var floorplanSize = ComputeFloorplanSize(DesiredFloorplanSize, DesiredFloorplanSize);
            _viewModel.SetFloorplanSize(floorplanSize);

            RefreshViewport();
        }

        private Size ComputeFloorplanSize(float desiredWidth, float desiredHeight)
        {
            var aspectRatioViewport = 900.0 / 720.0; // TODO: Hard coded values.  Compute this from viewport.
            var aspectRatioFloorplan = desiredWidth / desiredHeight;

            var floorplanWidth = desiredWidth;
            var floorplanHeight = desiredHeight;

            if (aspectRatioViewport > aspectRatioFloorplan)
            {
                floorplanWidth = (float)aspectRatioViewport * desiredHeight;
            }
            else
            {
                floorplanHeight = desiredWidth / (float)aspectRatioViewport;
            }

            var cameraFov = Angle.FromDegrees(SceneCamera.FieldOfView * 0.5f);
            var cameraDistance = (floorplanWidth * 0.5f) / System.Math.Tan(cameraFov.Radians);

            SceneCamera.Position = new WindowsPoint3D(0, 0, cameraDistance);

            return new Size(floorplanWidth, floorplanHeight);
        }

        private void OnPropertyChanged(object sender, EventArgs args)
        {
            RefreshViewport();
        }

        private void OnButtonAddClick(object sender, EventArgs args)
        {
            _viewModel.AddCamera();
        }
    }
}
