using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using MathNet.Spatial.Euclidean;


namespace CameraProjection
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly Floorplan _floorplan = new Floorplan();
        private readonly List<Camera> _cameras = new List<Camera>();

        public void SetFloorplanSize(Size size)
        {
            _floorplan.SetSize(size);
        }

        public void AddCamera()
        {
            var camera = new Camera();
            _cameras.Add(camera);
            SelectedCamera = camera;

            SendPropertyChanged(PropertySelectedCamera);
            SendPropertyChanged(PropertySelectedCameraYaw);
            SendPropertyChanged(PropertySelectedCameraPitch);
            SendPropertyChanged(PropertySelectedCameraRoll);
            SendPropertyChanged(PropertySelectedCameraHeight);
            SendPropertyChanged(PropertySelectedCameraFieldOfView);
            SendPropertyChanged(PropertySelectedCameraAspectRatio);
        }

        public Camera SelectedCamera { get; private set; }

        public float SelectedCameraYaw
        {
            get { return SelectedCamera == null ? 0 : SelectedCamera.Yaw; }
            set
            {
                SendPropertyChanged(PropertySelectedCameraYaw);
                SelectedCamera.Yaw = value;
            }
        }

        public float SelectedCameraPitch
        {
            get { return SelectedCamera == null ? 0 : SelectedCamera.Pitch; }
            set
            {
                SendPropertyChanged(PropertySelectedCameraPitch);
                SelectedCamera.Pitch = value;
            }
        }

        public float SelectedCameraRoll
        {
            get { return SelectedCamera == null ? 0 : SelectedCamera.Roll; }
            set
            {
                SendPropertyChanged(PropertySelectedCameraRoll);
                SelectedCamera.Roll = value;
            }
        }

        public double SelectedCameraHeight
        {
            get { return SelectedCamera == null ? 0 : SelectedCamera.Position.Z; }
            set
            {
                SendPropertyChanged(PropertySelectedCameraHeight);
                var position = SelectedCamera.Position;
                SelectedCamera.Position = new Point3D(position.X, position.Y, value);
            }
        }

        public float SelectedCameraFieldOfView
        {
            get { return SelectedCamera == null ? 0 : SelectedCamera.FieldOfView; }
            set
            {
                SendPropertyChanged(PropertySelectedCameraFieldOfView);
                SelectedCamera.FieldOfView = value;
            }
        }

        public float SelectedCameraAspectRatio
        {
            get { return SelectedCamera == null ? 0 : SelectedCamera.AspectRatio; }
            set
            {
                SendPropertyChanged(PropertySelectedCameraAspectRatio);
                SelectedCamera.AspectRatio = value;
            }
        }

        public IEnumerable<Projection> CameraProjections
        {
            get
            {
                var projections = new List<Projection>();

                foreach (var camera in _cameras)
                {
                    var projection = new Projection
                    {
                        Camera = camera,
                        Lines = _floorplan.ComputeFloorProjection(camera)
                    };

                    projections.Add(projection);
                }

                return projections;
            }
        }

       
        protected void SendPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                Debug.Assert(GetType().GetRuntimeProperty(property) != null);

                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private const string PropertySelectedCamera = "SelectedCamera";
        private const string PropertySelectedCameraYaw = "SelectedCameraYaw";
        private const string PropertySelectedCameraPitch = "SelectedCameraPitch";
        private const string PropertySelectedCameraRoll = "SelectedCameraRoll";
        private const string PropertySelectedCameraHeight = "SelectedCameraHeight";
        private const string PropertySelectedCameraFieldOfView = "SelectedCameraFieldOfView";
        private const string PropertySelectedCameraAspectRatio = "SelectedCameraAspectRatio";
    }
}
