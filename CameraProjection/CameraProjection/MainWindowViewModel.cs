using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;


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
        }

        public Camera SelectedCamera { get; private set; }

        public float SelectedCameraYaw
        {
            get { return SelectedCamera == null ? 0 : SelectedCamera.Yaw; }
            set
            {
                SendPropertyChanged(PropertySelectedCamera);
                SelectedCamera.Yaw = value;
            }
        }

        public float SelectedCameraPitch
        {
            get { return SelectedCamera == null ? 0 : SelectedCamera.Pitch; }
            set
            {
                SendPropertyChanged(PropertySelectedCamera);
                SelectedCamera.Pitch = value;
            }
        }

        public float SelectedCameraRoll
        {
            get { return SelectedCamera == null ? 0 : SelectedCamera.Roll; }
            set
            {
                SendPropertyChanged(PropertySelectedCamera);
                SelectedCamera.Roll = value;
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
    }
}
