using System.Diagnostics;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace MCG.Tools.CREOToolsFluentInterface.View
{
    public partial class EngineeringHubGlobe3D : UserControl
    {
        #region [REGION] Constants
        private const double SphereRadius = 1.1;
        private const int SphereStacks = 48;
        private const int SphereSlices = 96;

        private const double PinRadius = 0.03;   // taille des pins
        private const double PinAltitude = 1.13;   // distance au centre

        private const double MoonRadius = 0.27;   // ~27% de la Terre
        private const double MoonDistance = 2.2;    // rayon de l'orbite
        private const double MoonOrbitSpeed = 0.1;    // orbite LENTE et indépendante

        private const int OrbitDotCount = 90;     // densité de la traînée
        private const double OrbitDotRadius = 0.006;  // taille des points

        private const double AutoSpinSpeed = 0.3;    // rotation de la scène (Terre+Lune)
        private const double RotationSensitivity = 0.5;
        private const double ZoomFactorIn = 0.9;
        private const double ZoomFactorOut = 1.1;
        private const double DragThreshold = 4.0;
        private const double HitTolerancePx = 15.0;

        private static readonly TimeSpan SpinInterval = TimeSpan.FromMilliseconds(16);
        private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
        #endregion

        #region [REGION] Fields
        private Point _lastPos;
        private Point _downPos;
        private bool _isDragging;
        private DispatcherTimer? _spinTimer;   // auto-spin de la scène (s'arrête au clic)
        private DispatcherTimer? _idleTimer;   // relance de l'auto-spin après inactivité
        private DispatcherTimer? _moonTimer;   // orbite Lune (ne s'arrête jamais)

        private readonly Model3DGroup _worldGroup = new();
        private readonly Model3DGroup _moonGroup = new();
        private readonly Model3DGroup _orbitGroup = new();

        private sealed class PinInfo
        {
            public required string Culture { get; init; }
            public required Point3D LocalPos { get; init; }
            public required Color BaseColor { get; init; }
            public required GeometryModel3D Model { get; init; }
            public required DiffuseMaterial DiffuseMat { get; init; }
            public required EmissiveMaterial EmissiveMat { get; init; }
        }

        private readonly List<PinInfo> _pins = new();
        private PinInfo? _hoveredPin;
        #endregion

        #region [REGION] Events
        public event EventHandler<string>? LanguageSelected;
        #endregion

        #region [REGION] Constructor
        public EngineeringHubGlobe3D()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        #endregion

        #region [REGION] Lifecycle
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BuildWorld();
            WorldVisual.Content = _worldGroup;

            BuildOrbit();
            OrbitVisual.Content = _orbitGroup;

            BuildMoon();
            MoonVisual.Content = _moonGroup;

            EnsureSpinTimer();
            StartAutoSpin();

            EnsureMoonTimer();
            _moonTimer!.Start();   // la Lune démarre et ne s'arrêtera plus
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopAutoSpin();
            _idleTimer?.Stop();
            _moonTimer?.Stop();
        }
        #endregion

        #region [REGION] World build
        private void BuildWorld()
        {
            _worldGroup.Children.Clear();
            _pins.Clear();

            // 1) Globe
            _worldGroup.Children.Add(CreateGlobe());

            // 2) Pins pays avec couleurs
            var defs = new (string Culture, string Label, double Lat, double Lon, Color Color)[]
            {
                ("fr-FR", "France",  40.85,  -10.00, Colors.RoyalBlue),
                ("en-US", "USA",     32.90,  -89.04, Colors.Crimson),
                ("de-DE", "Germany", 44.52,    1.00, Colors.Goldenrod),
                ("zh-CN", "China",   30.90,  100.40, Colors.OrangeRed)
            };

            foreach (var d in defs)
            {
                var pos = LatLonToPoint(d.Lat, d.Lon, PinAltitude);
                var info = CreatePin(pos, PinRadius, d.Color, d.Culture);
                _worldGroup.Children.Add(info.Model);
                _pins.Add(info);
            }
        }

        private static GeometryModel3D CreateGlobe()
        {
            var mesh = new MeshGeometry3D();

            for (int stack = 0; stack <= SphereStacks; stack++)
            {
                double phi = Math.PI * stack / SphereStacks;
                double y = Math.Cos(phi) * SphereRadius;
                double r = Math.Sin(phi) * SphereRadius;

                for (int slice = 0; slice <= SphereSlices; slice++)
                {
                    double theta = 2 * Math.PI * slice / SphereSlices;
                    double x = r * Math.Cos(theta);
                    double z = r * Math.Sin(theta);

                    mesh.Positions.Add(new Point3D(x, y, z));
                    mesh.Normals.Add(new Vector3D(x, y, z));
                    mesh.TextureCoordinates.Add(new Point(
                        1.0 - (double)slice / SphereSlices,
                        (double)stack / SphereStacks));
                }
            }

            int stride = SphereSlices + 1;
            for (int stack = 0; stack < SphereStacks; stack++)
            {
                for (int slice = 0; slice < SphereSlices; slice++)
                {
                    int a = stack * stride + slice;
                    int b = a + 1;
                    int c = a + stride;
                    int d = c + 1;

                    mesh.TriangleIndices.Add(a);
                    mesh.TriangleIndices.Add(c);
                    mesh.TriangleIndices.Add(b);

                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(c);
                    mesh.TriangleIndices.Add(d);
                }
            }

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(
                "pack://application:,,,/MCG.CommonLib.Resources;component/Resources/world_map.png",
                UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            var material = new DiffuseMaterial(new ImageBrush(bmp));
            return new GeometryModel3D(mesh, material) { BackMaterial = material };
        }

        private static Point3D LatLonToPoint(double latDeg, double lonDeg, double r)
        {
            double lat = latDeg * Math.PI / 180.0;
            double lon = lonDeg * Math.PI / 180.0;
            double x = -r * Math.Cos(lat) * Math.Cos(lon);
            double y = r * Math.Sin(lat);
            double z = r * Math.Cos(lat) * Math.Sin(lon);
            return new Point3D(x, y, z);
        }
        #endregion

        #region [REGION] Moon + orbit build
        private void BuildMoon()
        {
            _moonGroup.Children.Clear();

            var mesh = new MeshGeometry3D();
            const int stacks = 24, slices = 48;

            for (int i = 0; i <= stacks; i++)
            {
                double phi = Math.PI * i / stacks;
                double y = Math.Cos(phi) * MoonRadius;
                double r = Math.Sin(phi) * MoonRadius;

                for (int j = 0; j <= slices; j++)
                {
                    double theta = 2 * Math.PI * j / slices;
                    double x = r * Math.Cos(theta);
                    double z = r * Math.Sin(theta);

                    mesh.Positions.Add(new Point3D(x, y, z));
                    mesh.Normals.Add(new Vector3D(x, y, z));
                    mesh.TextureCoordinates.Add(new Point(
                        1.0 - (double)j / slices,
                        (double)i / stacks));
                }
            }

            int stride = slices + 1;
            for (int i = 0; i < stacks; i++)
            {
                for (int j = 0; j < slices; j++)
                {
                    int a = i * stride + j;
                    int b = a + 1;
                    int c = a + stride;
                    int d = c + 1;
                    mesh.TriangleIndices.Add(a);
                    mesh.TriangleIndices.Add(c);
                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(c);
                    mesh.TriangleIndices.Add(d);
                }
            }

            Material material;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(
                    "pack://application:,,,/MCG.CommonLib.Resources;component/Resources/moon_map.png",
                    UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                material = new DiffuseMaterial(new ImageBrush(bmp));
            }
            catch
            {
                material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(200, 200, 200)));
            }

            // Décalage à la distance orbitale ; MoonOrbit (XAML) la fait tourner autour de la Terre
            var model = new GeometryModel3D(mesh, material)
            {
                BackMaterial = material,
                Transform = new TranslateTransform3D(MoonDistance, 0, 0)
            };

            _moonGroup.Children.Add(model);
        }

        private void BuildOrbit()
        {
            _orbitGroup.Children.Clear();

            var dotMaterial = new MaterialGroup();
            //dotMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(180, 180, 180))));
            //dotMaterial.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromRgb(120, 120, 120))));
            dotMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(90, 90, 90))));
            dotMaterial.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromRgb(50, 50, 50))));


            for (int k = 0; k < OrbitDotCount; k++)
            {
                double angle = 2 * Math.PI * k / OrbitDotCount;
                double x = MoonDistance * Math.Cos(angle);
                double z = MoonDistance * Math.Sin(angle);

                var dot = CreateSmallSphere(new Point3D(x, 0, z), OrbitDotRadius, dotMaterial);
                _orbitGroup.Children.Add(dot);
            }
        }

        private static GeometryModel3D CreateSmallSphere(Point3D center, double radius, Material material)
        {
            var mesh = new MeshGeometry3D();
            const int stacks = 6, slices = 8;

            for (int i = 0; i <= stacks; i++)
            {
                double phi = Math.PI * i / stacks;
                double y = Math.Cos(phi) * radius;
                double r = Math.Sin(phi) * radius;

                for (int j = 0; j <= slices; j++)
                {
                    double theta = 2 * Math.PI * j / slices;
                    mesh.Positions.Add(new Point3D(
                        center.X + r * Math.Cos(theta),
                        center.Y + y,
                        center.Z + r * Math.Sin(theta)));
                }
            }

            int stride = slices + 1;
            for (int i = 0; i < stacks; i++)
            {
                for (int j = 0; j < slices; j++)
                {
                    int a = i * stride + j;
                    int b = a + 1;
                    int c = a + stride;
                    int d = c + 1;
                    mesh.TriangleIndices.Add(a);
                    mesh.TriangleIndices.Add(c);
                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(c);
                    mesh.TriangleIndices.Add(d);
                }
            }

            return new GeometryModel3D(mesh, material);
        }
        #endregion

        #region [REGION] Pin creation + hover highlight
        private static PinInfo CreatePin(Point3D center, double radius, Color color, string culture)
        {
            // Petite sphère (couleur unie, pas besoin d'UV)
            var mesh = new MeshGeometry3D();
            const int stacks = 12, slices = 18;

            for (int i = 0; i <= stacks; i++)
            {
                double phi = Math.PI * i / stacks;
                double y = Math.Cos(phi) * radius;
                double r = Math.Sin(phi) * radius;

                for (int j = 0; j <= slices; j++)
                {
                    double theta = 2 * Math.PI * j / slices;
                    double x = r * Math.Cos(theta);
                    double z = r * Math.Sin(theta);
                    mesh.Positions.Add(new Point3D(x, y, z));
                }
            }

            int stride = slices + 1;
            for (int i = 0; i < stacks; i++)
            {
                for (int j = 0; j < slices; j++)
                {
                    int a = i * stride + j;
                    int b = a + 1;
                    int c = a + stride;
                    int d = c + 1;
                    mesh.TriangleIndices.Add(a);
                    mesh.TriangleIndices.Add(c);
                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(c);
                    mesh.TriangleIndices.Add(d);
                }
            }

            var diffuse = new DiffuseMaterial(new SolidColorBrush(color));
            var specular = new SpecularMaterial(new SolidColorBrush(Colors.White), 40);
            var emissive = new EmissiveMaterial(new SolidColorBrush(Colors.Black)); // OFF au départ

            var group = new MaterialGroup();
            group.Children.Add(diffuse);
            group.Children.Add(specular);
            group.Children.Add(emissive);

            var model = new GeometryModel3D(mesh, group)
            {
                Transform = new TranslateTransform3D(center.X, center.Y, center.Z)
            };

            return new PinInfo
            {
                Culture = culture,
                LocalPos = center,
                BaseColor = color,
                Model = model,
                DiffuseMat = diffuse,
                EmissiveMat = emissive
            };
        }

        private void SetHoveredPin(PinInfo? pin)
        {
            if (ReferenceEquals(_hoveredPin, pin)) return;

            // Reset ancien : éteindre l'émissif
            if (_hoveredPin != null)
                _hoveredPin.EmissiveMat.Brush = new SolidColorBrush(Colors.Black);

            _hoveredPin = pin;

            // Applique nouveau : halo = couleur de base éclaircie
            if (_hoveredPin != null)
            {
                var c = _hoveredPin.BaseColor;
                var glow = Color.FromRgb(
                    (byte)Math.Min(255, c.R + 80),
                    (byte)Math.Min(255, c.G + 80),
                    (byte)Math.Min(255, c.B + 80));
                _hoveredPin.EmissiveMat.Brush = new SolidColorBrush(glow);
            }

            Vp3D.Cursor = _hoveredPin != null ? Cursors.Hand : Cursors.Arrow;
        }
        #endregion

        #region [REGION] Mouse handlers
        private void Vp3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _downPos = e.GetPosition(Vp3D);
            _lastPos = _downPos;
            _isDragging = true;
            Vp3D.CaptureMouse();
            StopAutoSpin();
            _idleTimer?.Stop();
        }

        private void Vp3D_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            Vp3D.ReleaseMouseCapture();

            var upPos = e.GetPosition(Vp3D);
            var delta = upPos - _downPos;

            if (Math.Abs(delta.X) < DragThreshold && Math.Abs(delta.Y) < DragThreshold)
                TryHitPin(upPos);

            EnsureIdleTimer();
            _idleTimer!.Stop();
            _idleTimer.Start();
        }

        private void Vp3D_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(Vp3D);

            if (!_isDragging)
            {
                SetHoveredPin(FindPinAt(pos));
                return;
            }

            var dx = pos.X - _lastPos.X;
            var dy = pos.Y - _lastPos.Y;
            RotY.Angle += dx * RotationSensitivity;
            RotX.Angle += dy * RotationSensitivity;
            _lastPos = pos;
        }

        private void Vp3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var factor = e.Delta > 0 ? ZoomFactorIn : ZoomFactorOut;
            var pos = Cam.Position;
            Cam.Position = new Point3D(pos.X * factor, pos.Y * factor, pos.Z * factor);
        }
        #endregion

        #region [REGION] 2D hit test with tolerance
        private void TryHitPin(Point mousePos)
        {
            var hit = FindPinAt(mousePos);

            if (hit != null)
            {
                Debug.WriteLine($"[Globe] ✅ Pin cliqué : {hit.Culture}");
                LanguageSelected?.Invoke(this, hit.Culture);
            }
        }

        private PinInfo? FindPinAt(Point mousePos)
        {
            var worldMatrix = GetWorldMatrix();
            var camPos = Cam.Position;
            var toCenter = new Point3D(0, 0, 0) - camPos;
            double centerDist = toCenter.Length;

            PinInfo? best = null;
            double bestDist2 = HitTolerancePx * HitTolerancePx;

            foreach (var pin in _pins)
            {
                Point3D worldPos = worldMatrix.Transform(pin.LocalPos);

                var toPin = worldPos - camPos;
                double along = Vector3D.DotProduct(toPin, toCenter) / centerDist;
                if (along > centerDist) continue;

                if (!TryProject(worldPos, out Point screenPos)) continue;

                double dx = screenPos.X - mousePos.X;
                double dy = screenPos.Y - mousePos.Y;
                double d2 = dx * dx + dy * dy;

                if (d2 < bestDist2)
                {
                    bestDist2 = d2;
                    best = pin;
                }
            }
            return best;
        }

        private Matrix3D GetWorldMatrix()
        {
            var group = new Transform3DGroup();
            group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), RotX.Angle)));
            group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), RotY.Angle)));
            return group.Value;
        }

        private bool TryProject(Point3D worldPos, out Point screenPos)
        {
            screenPos = default;

            if (Vp3D.Camera is not PerspectiveCamera cam) return false;

            double w = Vp3D.ActualWidth;
            double h = Vp3D.ActualHeight;
            if (w <= 0 || h <= 0) return false;

            Vector3D zAxis = -cam.LookDirection; zAxis.Normalize();
            Vector3D xAxis = Vector3D.CrossProduct(cam.UpDirection, zAxis); xAxis.Normalize();
            Vector3D yAxis = Vector3D.CrossProduct(zAxis, xAxis);

            Vector3D eye = (Vector3D)cam.Position;
            var view = new Matrix3D(
                xAxis.X, yAxis.X, zAxis.X, 0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0,
                -Vector3D.DotProduct(xAxis, eye),
                -Vector3D.DotProduct(yAxis, eye),
                -Vector3D.DotProduct(zAxis, eye),
                1);

            Point3D pCam = view.Transform(worldPos);
            if (pCam.Z >= 0) return false;

            double aspect = w / h;
            double fovX = cam.FieldOfView * Math.PI / 180.0;
            double fx = 1.0 / Math.Tan(fovX / 2.0);
            double fy = fx * aspect;

            double xNdc = pCam.X * fx / -pCam.Z;
            double yNdc = pCam.Y * fy / -pCam.Z;

            screenPos = new Point(
                (xNdc + 1) * 0.5 * w,
                (1 - yNdc) * 0.5 * h);
            return true;
        }
        #endregion

        #region [REGION] Timers (auto-spin scène + orbite Lune)
        private void EnsureSpinTimer()
        {
            if (_spinTimer != null) return;
            _spinTimer = new DispatcherTimer { Interval = SpinInterval };
            _spinTimer.Tick += (s, e) => RotY.Angle += AutoSpinSpeed;   // SEULEMENT la scène
        }

        public void StartAutoSpin()
        {
            EnsureSpinTimer();
            _spinTimer!.Start();
        }

        public void StopAutoSpin() => _spinTimer?.Stop();

        private void EnsureIdleTimer()
        {
            if (_idleTimer != null) return;
            _idleTimer = new DispatcherTimer { Interval = IdleDelay };
            _idleTimer.Tick += (s, e) =>
            {
                _idleTimer!.Stop();
                StartAutoSpin();
            };
        }

        private void EnsureMoonTimer()
        {
            if (_moonTimer != null) return;
            _moonTimer = new DispatcherTimer { Interval = SpinInterval };
            _moonTimer.Tick += (s, e) => MoonOrbit.Angle += MoonOrbitSpeed; // orbite lente, jamais stoppée
        }
        #endregion
    }
    //public partial class EngineeringHubGlobe3D : UserControl
    //{
    //    #region [REGION] Constants
    //    private const double SphereRadius = 1.10;
    //    private const int SphereStacks = 48;
    //    private const int SphereSlices = 96;

    //    private const double PinRadius = 0.03;   // taille du pin
    //    private const double PinAltitude = 1.13;   // distance au centre => bien au-dessus de la sphère
    //    private const double HoverScale = 1.6;    // grossissement au survol

    //    private const double AutoSpinSpeed = 0.3;
    //    private const double RotationSensitivity = 0.5;
    //    private const double ZoomFactorIn = 0.9;
    //    private const double ZoomFactorOut = 1.1;
    //    private const double DragThreshold = 4.0;
    //    private const double HitTolerancePx = 15.0;

    //    private static readonly TimeSpan SpinInterval = TimeSpan.FromMilliseconds(16);
    //    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);


    //    private const double MoonRadius = 0.27;   // ~27% de la Terre (proportion réelle)
    //    private const double MoonDistance = 2.2;    // distance du centre de la Terre
    //    private const double MoonOrbitSpeed = 0.6;    // degrés/tick (plus rapide que la Terre)

    //    private const int OrbitDotCount = 90;      // nombre de points de la traînée
    //    private const double OrbitDotRadius = 0.012;   // taille de chaque point
    //    #endregion

    //    #region [REGION] Fields
    //    private Point _lastPos;
    //    private Point _downPos;
    //    private bool _isDragging;
    //    private DispatcherTimer? _spinTimer;
    //    private DispatcherTimer? _idleTimer;

    //    private readonly Model3DGroup _worldGroup = new();

    //    private readonly Model3DGroup _moonGroup = new();
    //    private readonly Model3DGroup _orbitGroup = new();
    //    private sealed class PinInfo
    //    {
    //        public required string Culture { get; init; }
    //        public required Point3D LocalPos { get; init; }
    //        public required Color BaseColor { get; init; }
    //        public required GeometryModel3D Model { get; init; }
    //        public required ScaleTransform3D Scale { get; init; }
    //        public required DiffuseMaterial DiffuseMat { get; init; }
    //        public required EmissiveMaterial EmissiveMat { get; init; }
    //    }

    //    private readonly List<PinInfo> _pins = new();
    //    private PinInfo? _hoveredPin;
    //    #endregion

    //    #region [REGION] Events
    //    public event EventHandler<string>? LanguageSelected;
    //    #endregion

    //    #region [REGION] Constructor
    //    public EngineeringHubGlobe3D()
    //    {
    //        InitializeComponent();
    //        Loaded += OnLoaded;
    //        Unloaded += OnUnloaded;
    //    }
    //    #endregion

    //    #region [REGION] Lifecycle
    //    private void OnLoaded(object sender, RoutedEventArgs e)
    //    {
    //        BuildWorld();
    //        WorldVisual.Content = _worldGroup;

    //        BuildMoon();
    //        MoonVisual.Content = _moonGroup;

    //        EnsureSpinTimer();
    //        StartAutoSpin();
    //    }

    //    private void OnUnloaded(object sender, RoutedEventArgs e)
    //    {
    //        StopAutoSpin();
    //        _idleTimer?.Stop();
    //    }
    //    #endregion

    //    #region [REGION] World build
    //    private void BuildWorld()
    //    {
    //        _worldGroup.Children.Clear();
    //        _pins.Clear();

    //        // 1) Globe
    //        _worldGroup.Children.Add(CreateGlobe());

    //        // 2) Pins pays (bien au-dessus de la surface : altitude 1.10)
    //        var defs = new (string Culture, string Label, double Lat, double Lon, Color Color)[]
    //        {
    //            ("fr-FR", "France",  40.85,  -10.00, Colors.RoyalBlue),
    //            ("en-US", "USA",     32.90,  -89.04, Colors.Crimson),
    //            ("de-DE", "Germany", 44.52,    1.00, Colors.Goldenrod),
    //            ("zh-CN", "China",   30.90,  100.40, Colors.OrangeRed)
    //        };

    //        foreach (var d in defs)
    //        {
    //            var pos = LatLonToPoint(d.Lat, d.Lon, PinAltitude);
    //            var info = CreatePin(pos, PinRadius, d.Color, d.Culture);
    //            _worldGroup.Children.Add(info.Model);
    //            _pins.Add(info);
    //        }
    //    }

    //    private void BuildMoon()
    //    {
    //        _moonGroup.Children.Clear();

    //        var mesh = new MeshGeometry3D();
    //        const int stacks = 24, slices = 48;

    //        for (int i = 0; i <= stacks; i++)
    //        {
    //            double phi = Math.PI * i / stacks;
    //            double y = Math.Cos(phi) * MoonRadius;
    //            double r = Math.Sin(phi) * MoonRadius;

    //            for (int j = 0; j <= slices; j++)
    //            {
    //                double theta = 2 * Math.PI * j / slices;
    //                double x = r * Math.Cos(theta);
    //                double z = r * Math.Sin(theta);

    //                mesh.Positions.Add(new Point3D(x, y, z));
    //                mesh.Normals.Add(new Vector3D(x, y, z));
    //                mesh.TextureCoordinates.Add(new Point(
    //                    1.0 - (double)j / slices,
    //                    (double)i / stacks));
    //            }
    //        }

    //        int stride = slices + 1;
    //        for (int i = 0; i < stacks; i++)
    //        {
    //            for (int j = 0; j < slices; j++)
    //            {
    //                int a = i * stride + j;
    //                int b = a + 1;
    //                int c = a + stride;
    //                int d = c + 1;
    //                mesh.TriangleIndices.Add(a);
    //                mesh.TriangleIndices.Add(c);
    //                mesh.TriangleIndices.Add(b);
    //                mesh.TriangleIndices.Add(b);
    //                mesh.TriangleIndices.Add(c);
    //                mesh.TriangleIndices.Add(d);
    //            }
    //        }

    //        Material material;

    //        // Texture lune si disponible, sinon gris uni
    //        try
    //        {
    //            var bmp = new BitmapImage();
    //            bmp.BeginInit();
    //            bmp.UriSource = new Uri(
    //                "pack://application:,,,/MCG.CommonLib.Resources;component/Resources/moon_map.png",
    //                UriKind.Absolute);
    //            bmp.CacheOption = BitmapCacheOption.OnLoad;
    //            bmp.EndInit();
    //            bmp.Freeze();
    //            material = new DiffuseMaterial(new ImageBrush(bmp));
    //        }
    //        catch
    //        {
    //            material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(200, 200, 200)));
    //        }

    //        // Décalage de la Lune à sa distance orbitale (sur l'axe X).
    //        // Le RotateTransform3D "MoonOrbit" du XAML la fait ensuite tourner autour de la Terre.
    //        var model = new GeometryModel3D(mesh, material)
    //        {
    //            BackMaterial = material,
    //            Transform = new TranslateTransform3D(MoonDistance, 0, 0)
    //        };

    //        _moonGroup.Children.Add(model);
    //    }




    //    private static GeometryModel3D CreateGlobe()
    //    {
    //        var mesh = new MeshGeometry3D();

    //        for (int stack = 0; stack <= SphereStacks; stack++)
    //        {
    //            double phi = Math.PI * stack / SphereStacks;
    //            double y = Math.Cos(phi) * SphereRadius;
    //            double r = Math.Sin(phi) * SphereRadius;

    //            for (int slice = 0; slice <= SphereSlices; slice++)
    //            {
    //                double theta = 2 * Math.PI * slice / SphereSlices;
    //                double x = r * Math.Cos(theta);
    //                double z = r * Math.Sin(theta);

    //                mesh.Positions.Add(new Point3D(x, y, z));
    //                mesh.Normals.Add(new Vector3D(x, y, z));
    //                mesh.TextureCoordinates.Add(new Point(
    //                    1.0 - (double)slice / SphereSlices,
    //                    (double)stack / SphereStacks));
    //            }
    //        }

    //        int stride = SphereSlices + 1;
    //        for (int stack = 0; stack < SphereStacks; stack++)
    //        {
    //            for (int slice = 0; slice < SphereSlices; slice++)
    //            {
    //                int a = stack * stride + slice;
    //                int b = a + 1;
    //                int c = a + stride;
    //                int d = c + 1;

    //                mesh.TriangleIndices.Add(a);
    //                mesh.TriangleIndices.Add(c);
    //                mesh.TriangleIndices.Add(b);

    //                mesh.TriangleIndices.Add(b);
    //                mesh.TriangleIndices.Add(c);
    //                mesh.TriangleIndices.Add(d);
    //            }
    //        }

    //        var bmp = new BitmapImage();
    //        bmp.BeginInit();
    //        bmp.UriSource = new Uri(
    //            "pack://application:,,,/MCG.CommonLib.Resources;component/Resources/world_map.png",
    //            UriKind.Absolute);
    //        bmp.CacheOption = BitmapCacheOption.OnLoad;
    //        bmp.EndInit();
    //        bmp.Freeze();

    //        var material = new DiffuseMaterial(new ImageBrush(bmp));
    //        return new GeometryModel3D(mesh, material) { BackMaterial = material };
    //    }

    //    private static Point3D LatLonToPoint(double latDeg, double lonDeg, double r)
    //    {
    //        double lat = latDeg * Math.PI / 180.0;
    //        double lon = lonDeg * Math.PI / 180.0;
    //        double x = -r * Math.Cos(lat) * Math.Cos(lon);
    //        double y = r * Math.Sin(lat);
    //        double z = r * Math.Cos(lat) * Math.Sin(lon);
    //        return new Point3D(x, y, z);
    //    }
    //    #endregion

    //    #region [REGION] Pin creation + hover highlight
    //    private static PinInfo CreatePin(Point3D center, double radius, Color color, string culture)
    //    {
    //        // Petite sphère centrée à (0,0,0) - la position est appliquée par TranslateTransform3D
    //        var mesh = new MeshGeometry3D();
    //        const int stacks = 12, slices = 18;

    //        for (int i = 0; i <= stacks; i++)
    //        {
    //            double phi = Math.PI * i / stacks;
    //            double y = Math.Cos(phi) * radius;
    //            double r = Math.Sin(phi) * radius;

    //            for (int j = 0; j <= slices; j++)
    //            {
    //                double theta = 2 * Math.PI * j / slices;
    //                double x = r * Math.Cos(theta);
    //                double z = r * Math.Sin(theta);
    //                mesh.Positions.Add(new Point3D(x, y, z));
    //            }
    //        }

    //        int stride = slices + 1;
    //        for (int i = 0; i < stacks; i++)
    //        {
    //            for (int j = 0; j < slices; j++)
    //            {
    //                int a = i * stride + j;
    //                int b = a + 1;
    //                int c = a + stride;
    //                int d = c + 1;
    //                mesh.TriangleIndices.Add(a);
    //                mesh.TriangleIndices.Add(c);
    //                mesh.TriangleIndices.Add(b);
    //                mesh.TriangleIndices.Add(b);
    //                mesh.TriangleIndices.Add(c);
    //                mesh.TriangleIndices.Add(d);
    //            }
    //        }

    //        var diffuse = new DiffuseMaterial(new SolidColorBrush(color));
    //        var specular = new SpecularMaterial(new SolidColorBrush(Colors.White), 40);
    //        var emissive = new EmissiveMaterial(new SolidColorBrush(Colors.Black)); // OFF au départ

    //        var group = new MaterialGroup();
    //        group.Children.Add(diffuse);
    //        group.Children.Add(specular);
    //        group.Children.Add(emissive);

    //        // Transform : scale (centré sur le pin) + translate à la position finale
    //        var scale = new ScaleTransform3D(1, 1, 1);
    //        var translate = new TranslateTransform3D(center.X, center.Y, center.Z);
    //        var tg = new Transform3DGroup();
    //        tg.Children.Add(scale);
    //        tg.Children.Add(translate);

    //        var model = new GeometryModel3D(mesh, group)
    //        {
    //            Transform = tg
    //        };

    //        return new PinInfo
    //        {
    //            Culture = culture,
    //            LocalPos = center,
    //            BaseColor = color,
    //            Model = model,
    //            Scale = scale,
    //            DiffuseMat = diffuse,
    //            EmissiveMat = emissive
    //        };
    //    }

    //    private void SetHoveredPin(PinInfo? pin)
    //    {
    //        if (ReferenceEquals(_hoveredPin, pin)) return;

    //        // Reset ancien
    //        if (_hoveredPin != null)
    //        {
    //            //_hoveredPin.Scale.ScaleX = 1;
    //            //_hoveredPin.Scale.ScaleY = 1;
    //            //_hoveredPin.Scale.ScaleZ = 1;
    //            _hoveredPin.EmissiveMat.Brush = new SolidColorBrush(Colors.Black);
    //        }

    //        _hoveredPin = pin;

    //        // Applique nouveau
    //        if (_hoveredPin != null)
    //        {
    //            //_hoveredPin.Scale.ScaleX = HoverScale;
    //            //_hoveredPin.Scale.ScaleY = HoverScale;
    //            //_hoveredPin.Scale.ScaleZ = HoverScale;

    //            var c = _hoveredPin.BaseColor;
    //            var glow = Color.FromRgb(
    //                (byte)Math.Min(255, c.R + 80),
    //                (byte)Math.Min(255, c.G + 80),
    //                (byte)Math.Min(255, c.B + 80));
    //            _hoveredPin.EmissiveMat.Brush = new SolidColorBrush(glow);
    //        }

    //        Vp3D.Cursor = _hoveredPin != null ? Cursors.Hand : Cursors.Arrow;
    //    }
    //    #endregion

    //    #region [REGION] Mouse handlers
    //    private void Vp3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    //    {
    //        _downPos = e.GetPosition(Vp3D);
    //        _lastPos = _downPos;
    //        _isDragging = true;
    //        Vp3D.CaptureMouse();
    //        StopAutoSpin();
    //        _idleTimer?.Stop();
    //    }

    //    private void Vp3D_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    //    {
    //        _isDragging = false;
    //        Vp3D.ReleaseMouseCapture();

    //        var upPos = e.GetPosition(Vp3D);
    //        var delta = upPos - _downPos;

    //        if (Math.Abs(delta.X) < DragThreshold && Math.Abs(delta.Y) < DragThreshold)
    //            TryHitPin(upPos);

    //        EnsureIdleTimer();
    //        _idleTimer!.Stop();
    //        _idleTimer.Start();
    //    }

    //    private void Vp3D_MouseMove(object sender, MouseEventArgs e)
    //    {
    //        var pos = e.GetPosition(Vp3D);

    //        if (!_isDragging)
    //        {
    //            SetHoveredPin(FindPinAt(pos));
    //            return;
    //        }

    //        var dx = pos.X - _lastPos.X;
    //        var dy = pos.Y - _lastPos.Y;
    //        RotY.Angle += dx * RotationSensitivity;
    //        RotX.Angle += dy * RotationSensitivity;
    //        _lastPos = pos;
    //    }

    //    private void Vp3D_MouseWheel(object sender, MouseWheelEventArgs e)
    //    {
    //        var factor = e.Delta > 0 ? ZoomFactorIn : ZoomFactorOut;
    //        var pos = Cam.Position;
    //        Cam.Position = new Point3D(pos.X * factor, pos.Y * factor, pos.Z * factor);
    //    }
    //    #endregion

    //    #region [REGION] 2D hit test with tolerance
    //    private void TryHitPin(Point mousePos)
    //    {
    //        var hit = FindPinAt(mousePos);

    //        if (hit != null)
    //        {
    //            Debug.WriteLine($"[Globe] ✅ Pin cliqué : {hit.Culture}");
    //            LanguageSelected?.Invoke(this, hit.Culture);
    //        }
    //        else
    //        {
    //            // Aide au diagnostic
    //            var worldMatrix = GetWorldMatrix();
    //            foreach (var pin in _pins)
    //            {
    //                var worldPos = worldMatrix.Transform(pin.LocalPos);
    //                if (TryProject(worldPos, out var sp))
    //                {
    //                    double d = Math.Sqrt(
    //                        Math.Pow(sp.X - mousePos.X, 2) +
    //                        Math.Pow(sp.Y - mousePos.Y, 2));
    //                    Debug.WriteLine(
    //                        $"[Globe] {pin.Culture} projeté ({sp.X:F0},{sp.Y:F0}) – " +
    //                        $"souris ({mousePos.X:F0},{mousePos.Y:F0}) – d={d:F1}px");
    //                }
    //            }
    //        }
    //    }

    //    private PinInfo? FindPinAt(Point mousePos)
    //    {
    //        var worldMatrix = GetWorldMatrix();
    //        var camPos = Cam.Position;
    //        var toCenter = new Point3D(0, 0, 0) - camPos;
    //        double centerDist = toCenter.Length;

    //        PinInfo? best = null;
    //        double bestDist2 = HitTolerancePx * HitTolerancePx;

    //        foreach (var pin in _pins)
    //        {
    //            Point3D worldPos = worldMatrix.Transform(pin.LocalPos);

    //            // Rejet des pins sur la face cachée
    //            var toPin = worldPos - camPos;
    //            double along = Vector3D.DotProduct(toPin, toCenter) / centerDist;
    //            if (along > centerDist) continue;

    //            if (!TryProject(worldPos, out Point screenPos)) continue;

    //            double dx = screenPos.X - mousePos.X;
    //            double dy = screenPos.Y - mousePos.Y;
    //            double d2 = dx * dx + dy * dy;

    //            if (d2 < bestDist2)
    //            {
    //                bestDist2 = d2;
    //                best = pin;
    //            }
    //        }
    //        return best;
    //    }

    //    private Matrix3D GetWorldMatrix()
    //    {
    //        var group = new Transform3DGroup();
    //        group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), RotX.Angle)));
    //        group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), RotY.Angle)));
    //        return group.Value;
    //    }

    //    private bool TryProject(Point3D worldPos, out Point screenPos)
    //    {
    //        screenPos = default;

    //        if (Vp3D.Camera is not PerspectiveCamera cam) return false;

    //        double w = Vp3D.ActualWidth;
    //        double h = Vp3D.ActualHeight;
    //        if (w <= 0 || h <= 0) return false;

    //        // View matrix (right-handed)
    //        Vector3D zAxis = -cam.LookDirection; zAxis.Normalize();
    //        Vector3D xAxis = Vector3D.CrossProduct(cam.UpDirection, zAxis); xAxis.Normalize();
    //        Vector3D yAxis = Vector3D.CrossProduct(zAxis, xAxis);

    //        Vector3D eye = (Vector3D)cam.Position;
    //        var view = new Matrix3D(
    //            xAxis.X, yAxis.X, zAxis.X, 0,
    //            xAxis.Y, yAxis.Y, zAxis.Y, 0,
    //            xAxis.Z, yAxis.Z, zAxis.Z, 0,
    //            -Vector3D.DotProduct(xAxis, eye),
    //            -Vector3D.DotProduct(yAxis, eye),
    //            -Vector3D.DotProduct(zAxis, eye),
    //            1);

    //        Point3D pCam = view.Transform(worldPos);
    //        if (pCam.Z >= 0) return false; // derrière la caméra

    //        // ⚠️ WPF : PerspectiveCamera.FieldOfView est HORIZONTAL
    //        double aspect = w / h;
    //        double fovX = cam.FieldOfView * Math.PI / 180.0;
    //        double fx = 1.0 / Math.Tan(fovX / 2.0);
    //        double fy = fx * aspect;

    //        double xNdc = pCam.X * fx / -pCam.Z;
    //        double yNdc = pCam.Y * fy / -pCam.Z;

    //        screenPos = new Point(
    //            (xNdc + 1) * 0.5 * w,
    //            (1 - yNdc) * 0.5 * h);
    //        return true;
    //    }
    //    #endregion

    //    #region [REGION] Auto-spin

    //    private void EnsureSpinTimer()
    //    {
    //        if (_spinTimer != null) return;
    //        _spinTimer = new DispatcherTimer { Interval = SpinInterval };
    //        _spinTimer.Tick += (s, e) =>
    //        {
    //            RotY.Angle += AutoSpinSpeed;      // rotation Terre
    //            MoonOrbit.Angle += MoonOrbitSpeed;     // ← orbite Lune
    //        };
    //    }
    //    //private void EnsureSpinTimer()
    //    //{
    //    //    if (_spinTimer != null) return;
    //    //    _spinTimer = new DispatcherTimer { Interval = SpinInterval };
    //    //    _spinTimer.Tick += (s, e) => RotY.Angle += AutoSpinSpeed;
    //    //}

    //    public void StartAutoSpin()
    //    {
    //        EnsureSpinTimer();
    //        _spinTimer!.Start();
    //    }

    //    public void StopAutoSpin() => _spinTimer?.Stop();

    //    private void EnsureIdleTimer()
    //    {
    //        if (_idleTimer != null) return;
    //        _idleTimer = new DispatcherTimer { Interval = IdleDelay };
    //        _idleTimer.Tick += (s, e) =>
    //        {
    //            _idleTimer!.Stop();
    //            StartAutoSpin();
    //        };
    //    }
    //    #endregion
    //}
}


