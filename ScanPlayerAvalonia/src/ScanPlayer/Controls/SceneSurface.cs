using System.Linq;
using AddUp.NCore.Printing;
using AddUp.NCore.Printing.Catalog;
using Avalonia.Input;
using Avalonia.Threading;
using ScanPlayer.Models;
using ScanPlayer.OpenGL;
using ScanPlayer.Rendering;

namespace ScanPlayer.Controls;

public class SceneSurface : GLSurface
{
    private readonly BaseRenderer renderer;
    private bool firstMouseMove = true;
    private bool mouseLeftButtonPressed;
    private bool mouseRighButtonPressed;
    private bool mouseMiddleButtonPressed;
    private (int x, int y) previousMouseLocation = (0, 0);
    private CursorScope? cursorScope;

    public SceneSurface() : this(true) { }
    public SceneSurface(bool enableTrackballRotation)
    {
        IsTrackballRotationEnabled = enableTrackballRotation;

        var model = new PrinterCatalogFactory().Create().Models.SingleOrDefault(m =>
            m.Family == PrinterFamily.FormUp350V2 &&
            m.HeadLayout.Heads.Count == 4 &&
            m.RecoatingSystem.Kind == RecoatingSystemKind.Roller);

        var def = new PrinterDefinitionBuilder(model).Build();

        var scene = new Scene
        {
            PrinterDefinition = def
        };

        var sceneOptions = new SceneOptions();
        sceneOptions.EnableHead(1, true);
        sceneOptions.EnableHead(2, true);
        sceneOptions.EnableHead(3, true);
        sceneOptions.EnableHead(4, true);

        renderer = new SceneRenderer(this, scene, sceneOptions);

        InitializeEvents();
    }

    private bool IsTrackballRotationEnabled { get; }

    protected override void OnInitializeRendering(GL gl, uint fb) => renderer.Initialize(gl);
    protected override void OnRender(GL gl, uint fb) => renderer.Render(gl);
    protected override void OnCleanupRendering(GL gl, uint fb) => renderer.Cleanup(gl);

    private void InitializeEvents()
    {
        GotFocus += (s, e) => Invalidate();
        LostFocus += (s, e) => Invalidate();

        PointerPressed += (s, e) =>
        {
            var properties = e.GetCurrentPoint(this).Properties;
            switch (properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    cursorScope = new CursorScope(this, StandardCursorType.Hand);
                    mouseLeftButtonPressed = true;
                    break;
                case PointerUpdateKind.MiddleButtonPressed:
                    mouseMiddleButtonPressed = true;
                    break;
                case PointerUpdateKind.RightButtonPressed:
                    if (IsTrackballRotationEnabled)
                        cursorScope = new CursorScope(this, StandardCursorType.SizeAll);
                    mouseRighButtonPressed = true;
                    break;
            }
        };

        PointerReleased += (s, e) =>
        {
            if (cursorScope != null)
            {
                cursorScope.Dispose();
                cursorScope = null;
            }

            var properties = e.GetCurrentPoint(this).Properties;
            switch (properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonReleased:
                    mouseLeftButtonPressed = false;
                    break;
                case PointerUpdateKind.MiddleButtonReleased:
                    mouseMiddleButtonPressed = false;
                    break;
                case PointerUpdateKind.RightButtonReleased:
                    mouseRighButtonPressed = false;
                    break;
            }
        };

        PointerMoved += (s, e) =>
        {
            var position = e.GetCurrentPoint(this).Position;
            var location = (x: (int)position.X, y: (int)position.Y);

            if (firstMouseMove)
            {
                firstMouseMove = false;
                previousMouseLocation = location;
                return;
            }

            var dx = location.x - previousMouseLocation.x;
            var dy = location.y - previousMouseLocation.y;
            previousMouseLocation = location;

            if (mouseLeftButtonPressed)
                renderer.Pan(dx, dy);
            else if (mouseRighButtonPressed && IsTrackballRotationEnabled)
                renderer.Rotate(dx, dy);
            else if (mouseMiddleButtonPressed)
            {
                // Nothing to do here...
            }
        };

        PointerWheelChanged += (s, e) =>
        {
            if ((e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
                renderer.Zoom((int)e.Delta.Y);
        };
    }

    private void Invalidate() => Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
}
