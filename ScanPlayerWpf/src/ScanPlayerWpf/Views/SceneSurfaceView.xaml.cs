using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HelixToolkit.Wpf.SharpDX;
using ScanPlayerWpf.Models;
using ScanPlayerWpf.Rendering;
using ScanPlayerWpf.ViewModels;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace ScanPlayerWpf.Views
{
    public partial class SceneSurfaceView : UserControl
    {
        private class Scene : IScene
        {
            public ISceneOptions Options { get; set; }
            public IPrinterDefinition Printer { get; set; }
            public IDrawingProgram Program { get; set; }
        }

        private readonly ISceneRenderer renderer;
        private readonly SceneNodeGroupModel3D sceneRoot;
        private readonly ProjectionCamera perspectiveCamera;
        private readonly ProjectionCamera orthographicCamera;

        public SceneSurfaceView()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            perspectiveCamera = new PerspectiveCamera
            {
                Position = new Point3D(0.0, 0.0, 450.0),
                LookDirection = new Vector3D(0.0, 0.0, -450.0),
                NearPlaneDistance = double.Epsilon,
                FarPlaneDistance = double.MaxValue
            };

            orthographicCamera = new OrthographicCamera
            {
                Position = new Point3D(0.0, 0.0, 450.0),
                LookDirection = new Vector3D(0.0, 0.0, -450.0),
                Width = 450.0,
                NearPlaneDistance = double.Epsilon,
                FarPlaneDistance = double.MaxValue
            };

            viewport3DX.BackgroundColor = Colors.White;
            viewport3DX.EffectsManager = new DefaultEffectsManager();
            viewport3DX.FXAALevel = FXAALevel.Low;
            viewport3DX.MSAA = MSAALevel.Maximum;
            viewport3DX.ModelUpDirection = new Vector3D(0.0, 0.0, 1.0);
            viewport3DX.ShowCoordinateSystem = true;
            viewport3DX.ShowViewCube = true;
            viewport3DX.IsPanEnabled = true;
            viewport3DX.ZoomAroundMouseDownPoint = false; // Does not work well...
            viewport3DX.EnableSwapChainRendering = true;
            viewport3DX.IsInertiaEnabled = false; // Nice camera movement, but has rendering side effects...

            viewport3DX.UseDefaultGestures = false;
            viewport3DX.InputBindings.Clear();
            viewport3DX.InputBindings.AddRange(new[]
            {
                new KeyBinding { Command = ViewportCommands.TopView, Key = Key.U },
                new KeyBinding { Command = ViewportCommands.BottomView, Key=Key.D },
                new KeyBinding { Command = ViewportCommands.FrontView, Key=Key.F },
                new KeyBinding { Command = ViewportCommands.BackView, Key=Key.B },
                new KeyBinding { Command = ViewportCommands.LeftView, Key=Key.L },
                new KeyBinding { Command = ViewportCommands.RightView, Key=Key.R },
                new KeyBinding(ViewportCommands.ZoomExtents, Key.E, ModifierKeys.Control),
                new KeyBinding(ViewportCommands.Reset, Key.R, ModifierKeys.Control)
            });

            viewport3DX.InputBindings.AddRange(new[]
            {
                new MouseBinding { Command = ViewportCommands.Rotate, MouseAction = MouseAction.RightClick },
                new MouseBinding { Command = ViewportCommands.Pan, MouseAction = MouseAction.LeftClick },
                new MouseBinding(ViewportCommands.ZoomExtents, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.Control)),
            });

            SetViewMode(is3d: false);

            viewport3DX.Items.Add(new AmbientLight3D { Color = Colors.White });            
            
            sceneRoot = new SceneNodeGroupModel3D();
            viewport3DX.Items.Add(new Element3DPresenter { Content = sceneRoot });

            renderer = new SceneRenderer(sceneRoot);

            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
        }

        private SceneViewModel SceneViewModel { get; set; }

        private void SetViewMode(bool is3d)
        {
            viewport3DX.ShowCoordinateSystem = is3d;
            viewport3DX.ShowViewCube = is3d;
            viewport3DX.IsRotationEnabled = is3d;

            if (is3d)
            {
                var cam = new PerspectiveCamera();
                perspectiveCamera.CopyTo(cam);
                viewport3DX.Orthographic = false;
                viewport3DX.Camera = cam;
            }
            else
            {
                var cam = new OrthographicCamera();
                orthographicCamera.CopyTo(cam);
                viewport3DX.Orthographic = true;
                viewport3DX.Camera = cam;
            }

            ZoomExtents();
        }

        private void Redraw(bool zoomExtents)
        {
            var scene = new Scene
            {
                Options = SceneViewModel.Workspace.SceneOptions,
                Printer = SceneViewModel.Workspace.Printer,
                Program = SceneViewModel.Workspace.DrawingProgram
            };

            renderer.Render(viewport3DX.ActualWidth, viewport3DX.ActualHeight, scene);
            RefreshVisibleNodes();

            if (zoomExtents) ZoomExtents();
        }

        private void ZoomExtents()
        {
            viewport3DX.ZoomExtents(0);
            if (viewport3DX.Camera is PerspectiveCamera)
            {
                // This fixes ZoomExtents. I don't know why but the platform appears too far...
                viewport3DX.Camera.Position = new Point3D(0.0, 0.0, 450.0);
                viewport3DX.Camera.LookDirection = new Vector3D(0.0, 0.0, -450.0);
            }
        }

        private void RefreshVisibleNodes()
        {
            // This tells for head-bound nodes whether they are visible or not
            var headNodes = SceneViewModel.Workspace.Printer.Heads.ToDictionary(
                h => NodeNames.GetHeadNodeName(h.Id),
                h => SceneViewModel.Workspace.SceneOptions.IsHeadEnabled(h.Id));

            foreach (var n in sceneRoot.GroupNode.Traverse())
            {
                if (n.Name == NodeNames.Reference)
                    n.Visible = SceneViewModel.Workspace.SceneOptions.ShowReference;
                else if (n.Name == NodeNames.Platform)
                    n.Visible = SceneViewModel.Workspace.SceneOptions.ShowPlatform;
                else if (n.Name == NodeNames.HeadReferences)
                    n.Visible = SceneViewModel.Workspace.SceneOptions.ShowHeadReferences;
                else if (n.Name == NodeNames.HeadFields)
                    n.Visible = SceneViewModel.Workspace.SceneOptions.ShowHeadFields;
                else if (n.Name == NodeNames.Jumps)
                    n.Visible = SceneViewModel.Workspace.SceneOptions.ShowJumps;
                else if (n.Name == NodeNames.Marks)
                    n.Visible = SceneViewModel.Workspace.SceneOptions.ShowMarks;
                else if (n.Name == NodeNames.Points)
                    n.Visible = SceneViewModel.Workspace.SceneOptions.ShowPoints;
                else if (headNodes.ContainsKey(n.Name))
                    n.Visible = headNodes[n.Name];
            }
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            SceneViewModel = DataContext as SceneViewModel;
            WireViewModelPropertyChangedEvents();
            Redraw(true);
            DataContextChanged += OnDataContextChanged;
        }

        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            DataContextChanged -= OnDataContextChanged;
            UnwireViewModelPropertyChangedEvents();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnwireViewModelPropertyChangedEvents();
            SceneViewModel = DataContext as SceneViewModel;
            WireViewModelPropertyChangedEvents();
        }

        private void WireViewModelPropertyChangedEvents()
        {
            if (SceneViewModel == null) return;
            SceneViewModel.Workspace.PropertyChanged += OnWorkspacePropertyChanged;
            SceneViewModel.Workspace.SceneOptions.PropertyChanged += OnSceneOptionsPropertyChanged;
            SceneViewModel.Workspace.SceneOptions.EnabledHeadsChanged += OnSceneOptionsEnabledHeadsChanged;
        }

        private void UnwireViewModelPropertyChangedEvents()
        {
            if (SceneViewModel == null) return;
            SceneViewModel.Workspace.PropertyChanged -= OnWorkspacePropertyChanged;
            SceneViewModel.Workspace.SceneOptions.PropertyChanged -= OnSceneOptionsPropertyChanged;
            SceneViewModel.Workspace.SceneOptions.EnabledHeadsChanged -= OnSceneOptionsEnabledHeadsChanged;
        }

        private void OnSceneOptionsPropertyChanged(object sender, PropertyChangedEventArgs e) => RefreshVisibleNodes();
        private void OnSceneOptionsEnabledHeadsChanged(object sender, EventArgs e) => RefreshVisibleNodes();
        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Workspace.Project))
                Redraw(true);
        }

        private void RedrawButton_Click(object sender, RoutedEventArgs e) => Redraw(false);
        private void OrthoButton_Click(object sender, RoutedEventArgs e) => SetViewMode(is3d: false);
        private void PerspectiveButton_Click(object sender, RoutedEventArgs e) => SetViewMode(is3d: true);
    }
}
