using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace MCG.Tools.CREOToolsFluentInterface.View
{

    public partial class EngineeringHubCube3D : UserControl
    {
        #region [REGION] Fields

        private Point _lastPos;
        private bool _isDragging;

        private DispatcherTimer _spinTimer;
        private DispatcherTimer _idleTimer;

        private const double AutoSpinSpeed = 0.3;     // degrés / tick
        private const double RotationSensitivity = 0.5;
        private const double ZoomFactorIn = 0.9;
        private const double ZoomFactorOut = 1.1;

        private static readonly TimeSpan SpinInterval = TimeSpan.FromMilliseconds(16); // ~60 FPS
        private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);

        #endregion

        #region [REGION] Constructor

        public EngineeringHubCube3D()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #endregion

        #region [REGION] Lifecycle

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            EnsureSpinTimer();
            StartAutoSpin();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopAutoSpin();
            _idleTimer?.Stop();
        }

        #endregion

        #region [REGION] Mouse handlers

        private void Vp3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _lastPos = e.GetPosition(Vp3D);
            _isDragging = true;
            Vp3D.CaptureMouse();

            StopAutoSpin();
            _idleTimer?.Stop();
        }

        private void Vp3D_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            Vp3D.ReleaseMouseCapture();

            // ⏱ Relance l'auto-spin après 5s d'inactivité
            EnsureIdleTimer();
            _idleTimer.Stop();
            _idleTimer.Start();
        }

        private void Vp3D_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            var pos = e.GetPosition(Vp3D);
            var dx = pos.X - _lastPos.X;
            var dy = pos.Y - _lastPos.Y;

            RotY.Angle += dx * RotationSensitivity;   // ✅ Maintenant ça marche
            RotX.Angle += dy * RotationSensitivity;

            _lastPos = pos;
        }

        private void Vp3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var factor = e.Delta > 0 ? ZoomFactorIn : ZoomFactorOut;
            var pos = Cam.Position;

            Cam.Position = new Point3D(
                pos.X * factor,
                pos.Y * factor,
                pos.Z * factor);
        }

        #endregion

        #region [REGION] Auto-spin

        private void EnsureSpinTimer()
        {
            if (_spinTimer != null) return;

            _spinTimer = new DispatcherTimer { Interval = SpinInterval };
            _spinTimer.Tick += (s, e) => RotY.Angle += AutoSpinSpeed;
        }

        public void StartAutoSpin()
        {
            EnsureSpinTimer();
            _spinTimer.Start();
        }

        public void StopAutoSpin()
        {
            _spinTimer?.Stop();
        }

        private void EnsureIdleTimer()
        {
            if (_idleTimer != null) return;

            _idleTimer = new DispatcherTimer { Interval = IdleDelay };
            _idleTimer.Tick += (s, e) =>
            {
                _idleTimer.Stop();
                StartAutoSpin();
            };
        }

        #endregion
    }
}
